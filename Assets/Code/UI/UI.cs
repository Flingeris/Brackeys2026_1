using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
    [SerializeField] private GameObject winScreen;
    [SerializeField] private GameObject loseScreen;
    [SerializeField] private GameObject titleScreen;

    public Image TitleScreenImage => titleScreen.GetComponent<Image>();


    private void Awake()
    {
        if (G.UI != null && G.UI != this)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

        G.UI = this;

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (winScreen.activeSelf)
            G.ScreenFader.FadeOutCustom(winScreen.GetComponent<Image>(), 0f, () => SetWinActive(false));
    }


    public void SetWinActive(bool win)
    {
        Debug.Log($"[SetWinActive] this={name}, winScreen={(winScreen ? winScreen.name : "NULL")}");
        if (winScreen == null) return;
        winScreen.SetActive(win);
    }

    public void SetLoseActive(bool win)
    {
        if (loseScreen == null) return;
        loseScreen.SetActive(win);
    }

    public void ToggleTitle(bool b)
    {
        if (titleScreen == null) return;
        titleScreen.SetActive(b);
    }

    public void HideAllUI(bool hide)
    {
        if (winScreen != null) winScreen.SetActive(!hide);
        if (loseScreen != null) loseScreen.SetActive(!hide);
        if (titleScreen != null) titleScreen.SetActive(!hide);
    }

    private void OnDestroy()
    {
        Debug.Log($"[UI OnDestroy] {GetInstanceID()} on {gameObject.name}");
    }
}