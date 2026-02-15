using System;
using System.Collections;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.PlayerLoop;
using UnityEngine.SceneManagement;


public class RunState
{
}

public class Main : MonoBehaviour
{
    public static event UnityAction OnGameReady;
    public static bool TitleShown;
    public bool ShowTitle;

    private void Awake()
    {
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
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            G.Hand.Draw();
        }
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


    public void QuitGame()
    {
        Application.Quit();
    }
}

public class Pile : MonoBehaviour
{
}


public class Field : MonoBehaviour
{
    [SerializeField] private FieldCardSlot[] cardsSlots;


    private void OnValidate()
    {
      if(cardsSlots == null ||  cardsSlots.Length == 0 || cardsSlots.Length != transform.childCount) 
          cardsSlots = GetComponentsInChildren<FieldCardSlot>();
    }
}


