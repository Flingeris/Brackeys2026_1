using UnityEngine;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
    [SerializeField] private GameObject winScreen;
    [SerializeField] private GameObject loseScreen;
    [SerializeField] private GameObject titleScreen;

    public Image TitleScreenImage => titleScreen.GetComponent<Image>();


    private void Awake()
    {
        G.UI = this;
    }

    private void Start()
    {
    }

    public void SetWinActive(bool win)
    {
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
}