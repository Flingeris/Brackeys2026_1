using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.LowLevelPhysics;
using UnityEngine.PlayerLoop;
using UnityEngine.SceneManagement;
using Random = System.Random;


public class RunState
{
    public List<CardState> currDeck;
    public List<MemberState> CharactersStates;


    public int drawSize = 5;

    public int mapNodeIndex = -1;
}

public class Main : MonoBehaviour
{
    public bool IsChoosingTarget;
    public static event UnityAction OnGameReady;
    public static bool TitleShown;
    public bool ShowTitle;
    public Field field;


    private List<CardState> drawPile;
    private List<CardState> discardPile;
    [SerializeField] private List<CardModel> startDeck;

    private void Awake()
    {
        G.main = this;

        if (G.run == null) G.run = new RunState();
        var cards = startDeck.Select(cardModel => new CardState(cardModel)).ToList();

        G.run.currDeck = new List<CardState>(cards);
        drawPile = new List<CardState>(G.run.currDeck);
        discardPile = new List<CardState>();
    }

    private void Start()
    {
        CMS.Init();
        Debug.Log("OnGameReady");
        OnGameReady?.Invoke();

        StartCoroutine(GameStartSequence());
    }

    private IEnumerator GameStartSequence()
    {
        if (ShowTitle && !TitleShown) yield return ShowTitleScreen();

#if !UNITY_EDITOR
       while (!ServiceMain.ServicesReady) yield return null;
#endif
        yield return LoadLvl();
        G.audioSystem.Play(SoundId.Ambient_Sewer);

        StartTurn();
    }

    private IEnumerator LoadLvl()
    {
        var characters = CMS.GetAll<MemberModel>();
        foreach (var character in characters)
        {
            G.party.AddMember(character);
        }

        var enemies = CMS.GetAll<EnemyModel>().ToList();
        for (int i = 0; i <= 2; i++)
        {
            var enemy = enemies.GetRandomElement();
            G.enemies.AddEnemy(enemy);
        }

        yield break;
    }


    public void StartTurn()
    {
        StartCoroutine(StartTurnSequence());
    }

    private IEnumerator StartTurnSequence()
    {
        turns = BuildTurnOrder();

        G.run.currDeck.Shuffle();
        yield return DrawCards();

        // G.Hand.DrawControlledHand(G.run.drawSize);

        G.HUD.SetEndTurnInteractable(true);

        yield return null;
    }

    public IEnumerator DrawCards()
    {
        for (int i = 0; i < G.run.drawSize; i++)
        {
            if (drawPile.Count == 0)
            {
                drawPile.AddRange(discardPile);
                discardPile.Clear();
                drawPile.Shuffle();
            }

            var cardState = drawPile.Pop();
            G.Hand.AddCard(cardState);

            yield return new WaitForSeconds(0.1f);
        }

        yield break;
    }


    private List<ITurnEntity> BuildTurnOrder()
    {
        var result = new List<ITurnEntity>();
        result.AddRange(G.enemies.GetAliveEnemies());
        result.AddRange(field.cardsSlots.Where(c => c != null));

        result = result.OrderByDescending(e => e.Speed).ToList();

        for (int i = 0; i < result.Count; i++)
        {
            if (result[i] is MonoBehaviour mb && mb == null) continue;
            result[i].SetTurnIndex(i + 1);
        }


        return result;
    }

    public void EndTurn()
    {
        StartCoroutine(EndTurnSequence());
    }


    public List<ITurnEntity> turns;

    private IEnumerator EndTurnSequence()
    {
        G.HUD.SetEndTurnInteractable(false);


        foreach (var turn in turns)
        {
            if (turn is MonoBehaviour mb && mb == null) continue;
            yield return turn.OnTurnEnd();
            if (CheckForWin())
            {
                yield return WinSequence();
                yield break;
            }

            yield return new WaitForSeconds(0.25f);
        }

        yield return FieldClearSequence();
        yield return StartTurnSequence();
    }


    public bool CheckForWin()

    {
        return G.enemies.AllMembersDead();
    }


    public void GameLost()
    {
        StartCoroutine(LoseSequence());
    }

    public IEnumerator LoseSequence()
    {
        G.UI.SetLoseActive(true);

        yield return new WaitForSecondsRealtime(2f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }


    private IEnumerator WinSequence()
    {
        G.UI.SetWinActive(true);

        yield return new WaitForSecondsRealtime(2f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public IEnumerator FieldClearSequence()

    {
        yield return G.Hand.Clear();
        yield return field.Clear();
        yield return null;
    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            var party = G.party.GetAliveMembers();
            foreach (var member in party)
            {
                member.Kill();
            }
        }
    }


    public IEnumerator KillCard(CardInstance card)
    {
        if (card == null) yield break;
        card.Leave();

        card.transform.DOKill();
        card.Draggable.transform.DOKill();
            
        card.transform.DOMove(new Vector3(12, -3, transform.position.z), 0.1f);
        card.transform.DOScale(0f, 0.1f);

        yield return new WaitForSeconds(0.1f);
        discardPile.Add(card.state);
        Destroy(card.gameObject);
    }


    public void PlayCard(CardInstance card)
    {
        StartCoroutine(OnCardPlayed(card));
    }

    public IEnumerator OnCardPlayed(CardInstance card)
    {
        yield return card.OnCardPlayed();
    }

    private IEnumerator ShowTitleScreen()
    {
        G.UI.ToggleTitle(true);
        // G.audioSystem.Play(SoundId.SFX_LevelTransiton);

        yield return new WaitForSeconds(2f);
        G.ScreenFader.FadeOutCustom(G.UI.TitleScreenImage, 2f);
        TitleShown = true;
        yield break;
    }


    public void TryChooseTarget(ICombatEntity target)
    {
        if (!IsChoosingTarget) return;
        if (target == null) return;

        if (possibleTargets.Contains(target)) Target = target;
    }

    public ICombatEntity Target;
    public List<ICombatEntity> possibleTargets;

    public IEnumerator ChooseTarget(TargetSide targetSide, int[] targetsPos)
    {
        IsChoosingTarget = true;
        Target = null;

        CheckTargets(targetSide, targetsPos);

        if (possibleTargets.Count > 0)
        {
            while (Target == null)
                yield return null;
        }

        IsChoosingTarget = false;
        G.party.UntargetAll();
        G.enemies.UntargetAll();
    }

    public void CheckTargets(TargetSide targetSide, int[] possiblePos)
    {
        if (!IsChoosingTarget) return;
        List<ICombatEntity> targets;
        switch (targetSide)
        {
            case TargetSide.Any:
                targets = G.party.GetAliveMembers();
                targets.AddRange(G.enemies.GetAliveMembers());
                break;
            case TargetSide.Allies:
                targets = G.party.GetMembers(possiblePos).ToList();
                break;
            case TargetSide.Enemies:
                targets = G.enemies.GetMembers(possiblePos).ToList();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        foreach (var target in targets)
        {
            if (target == null) continue;
            target.SetTarget(true);
        }

        possibleTargets = targets;
    }


    public void QuitGame()
    {
        Application.Quit();
    }
}

internal class LevelModel : ScriptableObject
{
}

public class Pile : MonoBehaviour
{
}