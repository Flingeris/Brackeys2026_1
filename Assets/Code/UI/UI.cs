using UnityEngine;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
    [SerializeField] private GameObject winScreen;
    [SerializeField] private GameObject loseScreen;
    [SerializeField] private GameObject titleScreen;

    public Image TitleScreenImage => titleScreen.GetComponent<Image>();

    [SerializeField] private Button exitGameButton;

    private void Awake()
    {
        G.ui = this;
    }

    private void Start()
    {
        exitGameButton.onClick.AddListener(G.main.QuitGame);
    }

    public void SetWin(bool win)
    {
        if (winScreen == null) return;
        winScreen.SetActive(win);
    }

    public void SetLose(bool win)
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