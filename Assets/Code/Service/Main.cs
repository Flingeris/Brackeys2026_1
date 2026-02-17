using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.PlayerLoop;
using UnityEngine.SceneManagement;
using Random = System.Random;


public class RunState
{
}

public class Main : MonoBehaviour
{
    public int DrawSize = 6;
    public static event UnityAction OnGameReady;
    public static bool TitleShown;
    public bool ShowTitle;
    public Field field;

    public bool IsChoosingTarget;

    private void Awake()
    {
        G.main = this;
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

        StartTurn();
    }

    private IEnumerator LoadLvl()
    {
        var characters = CMS.GetAll<MemberModel>();
        foreach (var character in characters)
        {
            G.party.AddMember(character);
        }

        for (int i = 0; i <= 2; i++)
        {
            var enemies = CMS.GetAll<EnemyModel>();
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
        for (int i = 0; i < DrawSize; i++)
        {
            G.Hand.Draw();
        }

        G.HUD.SetEndTurnInteractable(true);

        yield return null;
    }


    public void EndTurn()
    {
        StartCoroutine(EndTurnSequence());
    }

    private IEnumerator EndTurnSequence()
    {
        G.HUD.SetEndTurnInteractable(false);

        var playerCards = field.PlayedCards;
        foreach (var card in playerCards)
        {
            if (card == null) continue;
            yield return card.OnCardTurnEnd();
            if (CheckForWin())
            {
                yield return WinSequence();
                yield break;
            }
        }


        var enemies = G.enemies.GetAliveEnemies();
        foreach (var enemy in enemies)
        {
            var startPos = enemy.transform.position;
            var endPos = startPos;
            endPos.x -= 3f;
            enemy.transform.DOMove(endPos, 0.1f);
            yield return new WaitForSecondsRealtime(0.1f);
            enemy.transform.DOMove(startPos, 0.1f);

            G.party.GetRandomMember().TakeDamage(enemy.CurrDmg);
            yield return new WaitForSeconds(0.1f);
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
        G.Hand.Clear();
        field.Clear();
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
        // G.ui.ToggleTitle(true);
        // G.audioSystem.Play(SoundId.SFX_LevelTransiton);
        //
        // yield return new WaitForSeconds(2f);
        // G.ScreenFader.FadeOutCustom(G.ui.TitleScreenImage, 2f);
        // TitleShown = true;
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