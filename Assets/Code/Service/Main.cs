using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;


public class RunState
{
    public List<CardState> currDeck = new();
    public List<MemberState> CharactersStates = new();


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
    [SerializeField] private SpriteRenderer background;

    private void Awake()
    {
        G.main = this;

        if (G.run == null)
        {
            G.run = new RunState();
            var cards = startDeck.Select(cardModel => new CardState(cardModel)).ToList();
            G.run.mapNodeIndex++;
            G.run.currDeck = new List<CardState>(cards);

            var characters = CMS.GetAll<CharacterModel>();
            foreach (var character in characters)
            {
                G.run.CharactersStates.Add(new MemberState(character));
            }
        }

        Draggable.DisableInteractionGlobal = false;
        drawPile = new List<CardState>(G.run.currDeck.Shuffle());
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
        yield return LoadLvlFromIndex(G.run.mapNodeIndex);
        G.audioSystem.Play(SoundId.Ambient_Sewer);

        yield return StartTurnSequence();


        if (G.run.mapNodeIndex == 0 && PlayerPrefs.GetInt("tutor1", 0) == 0)
        {
            yield return FirstTutorialSequence();
        }
    }


    public IEnumerator FirstTutorialSequence()
    {
        G.HUD.SetEndTurnInteractable(false);    
        
        G.HUD.tutorial.SetTutorialText("This is your actions");
        G.HUD.tutorial.ShowHand();

        yield return G.HUD.tutorial.WaitForSkip();

        G.HUD.tutorial.SetTutorialText("Drag them into action slots to play", 200);
        G.HUD.tutorial.ShowField();

        yield return G.HUD.tutorial.WaitForSkip();

        Debug.Log(field.PlayedCards.Length);
        while (field.PlayedCards.Length == 0)
        {
            Debug.Log("Waiting for field");
            yield return new WaitForEndOfFrame();
        }

        G.HUD.tutorial.SetTutorialText("Red numbers on the floor are turn order", 300);
        G.HUD.tutorial.ShowNumbers();
        G.HUD.SetTurnOrderHighlight(true);

        yield return G.HUD.tutorial.WaitForSkip();

        G.HUD.SetTurnOrderHighlight(false);

        G.HUD.tutorial.SetTutorialText("When you are ready press end turn");
        G.HUD.tutorial.ShowEndTurn();

        yield return G.HUD.tutorial.WaitForSkip();

        yield return new WaitForSeconds(1f);
        
        G.HUD.SetEndTurnInteractable(true);   

        PlayerPrefs.SetInt("tutor1", 1);
    }

    public void StartSecondTutorial()
    {
        StartCoroutine(SecondTutorialSequence());
    }

    private IEnumerator SecondTutorialSequence()
    {
        G.HUD.tutorial.SetTutorialText("If a character dies, actions of their type can still appear, but less often", 0, 300);
        G.HUD.tutorial.ShowCharacters();

        yield return G.HUD.tutorial.WaitForSkip();

        G.HUD.tutorial.SetTutorialText("Winning a battle revives the character for the next fight, but lowers their max health",0,300);
        G.HUD.tutorial.ShowCharacters();
        
        yield return G.HUD.tutorial.WaitForSkip();
        
        PlayerPrefs.SetInt("tutor2", 1);
    }


    private IEnumerator LoadLvlFromIndex(int lvlIndex)
    {
        var allCards = CMS.GetAll<CardModel>();
        var allLvls = CMS.GetAll<LevelModel>();

        var matchedLvls = allLvls.Where(l => l.lvlIndex == lvlIndex).ToList();

        if (!matchedLvls.Any())
        {
            Debug.LogWarning("No lvls found for lvlindex " + lvlIndex);
            yield break;
        }

        var lvl = matchedLvls.GetRandomElement();
        yield return LoadLvl(lvl);

        yield break;
    }

    private IEnumerator LoadLvl(LevelModel lvl)
    {
        if (lvl == null || !lvl.enemies.Any())
        {
            Debug.LogError("lvl is null or empty " + lvl?.name);
            yield break;
        }

        background.sprite = lvl.backgroundSprite;
        G.audioSystem.Play(lvl.LevelAmbient);


        var characters = G.run.CharactersStates;

        foreach (var character in characters)
        {
            var member = G.party.CreateMember(character);
            G.party.AddMember(member, member.state.currPos);
        }

        var enemies = lvl.enemies;
        for (int i = 0; i <= enemies.Length - 1; i++)
        {
            var enemy = enemies[i];
            if (enemy == null) continue;
            G.enemies.AddEnemy(enemy, i);
        }
    }


    public void StartTurn()
    {
        StartCoroutine(StartTurnSequence());
    }

    private IEnumerator StartTurnSequence()
    {
        turns = BuildTurnOrder();

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
            if (cardState.CardOwner == null || cardState.CardOwner.IsDead)
            {
                i--;
                continue;
            }

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
        Draggable.DisableInteractionGlobal = true;

        foreach (var m in G.party.GetAliveMembers())
        {
            yield return m.EndTurnStatusTick();
        }

        foreach (var turn in turns)
        {
            if (turn is MonoBehaviour mb && mb == null) continue;
            yield return turn.OnTurn();
            if (CheckForWin())
            {
                yield return WinSequence();
                yield break;
            }

            yield return new WaitForSeconds(0.35f);
        }

        yield return FieldClearSequence();
        Draggable.DisableInteractionGlobal = false;
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
        var allLvls = CMS.GetAll<LevelModel>();
        var matchedLvls = allLvls.Where(l => l.lvlIndex == G.run.mapNodeIndex + 1).ToList();

        if (!matchedLvls.Any())
        {
            G.UI.SetWinActive(true);
            yield return new WaitForSecondsRealtime(2f);
            G.run = null;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            yield break;
        }


        yield return FieldClearSequence();
        SetVisualActive(false);
        yield return G.Reward.StartRewarding<CardModel>();

        G.run.mapNodeIndex++;

        yield return G.ScreenFader.LevelTransition();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public IEnumerator FieldClearSequence()
    {
        yield return G.Hand.Clear();
        yield return field.Clear();
        yield return null;
    }


    public void SetVisualActive(bool b)
    {
        field.gameObject.SetActive(b);
        G.party.gameObject.SetActive(b);
        G.HUD.SetVisible(b);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            G.run = null;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            G.Hand.AddCard(new CardState(CMS.Get<CardModel>("vuln")));
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }if (Input.GetKeyDown(KeyCode.Y))
        {
            G.run.mapNodeIndex++;
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

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            var enemies = G.enemies.GetAliveEnemies();
            foreach (var enemyInstance in enemies)
            {
                enemyInstance.Kill();
            }

            EndTurn();
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            StartCoroutine(KillTarget());
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            G.Hand.AddCard(new CardState(CMS.Get<CardModel>("vuln")));
        }
    }


    public IEnumerator KillTarget()
    {
        yield return ChooseTarget(TargetSide.Any, null);

        if (Target != null && !Target.IsDead) Target.Kill();
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
                var taunted = G.enemies.GetMembersWithStatus(StatusEffectType.Taunt);
                if (taunted.Count > 0)
                {
                    targets = taunted;
                }
                else
                {
                    targets = G.enemies.GetMembers(possiblePos).ToList();
                }

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