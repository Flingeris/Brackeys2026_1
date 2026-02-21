using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    [SerializeField] public Tooltip tooltip;
    [SerializeField] private PauseManager pauseManager;
    [SerializeField] private TMP_Text sayText;

    [Header("Buttons")] [SerializeField] private Button EndTurnButton;


    //Runtime
    private Coroutine sayRoutine;

    private bool skipType;

    private void OnValidate()
    {
        if (pauseManager == null) pauseManager = FindAnyObjectByType<PauseManager>();
    }

    private void Awake()
    {
        G.HUD = this;
    }

    private void Start()
    {
        EndTurnButton.onClick.AddListener(G.main.EndTurn);
    }


    public void SetVisible(bool b)
    {
        EndTurnButton.gameObject.SetActive(b);
    }


    public void SetEndTurnInteractable(bool b)
    {
        EndTurnButton.interactable = b;
    }


    public Coroutine Say(string text, float charDelay = 0.02f)
    {
        if (sayRoutine != null)
            StopCoroutine(sayRoutine);

        sayRoutine = StartCoroutine(SayRoutine(text, charDelay));
        return sayRoutine;
    }

    private IEnumerator SayRoutine(string text, float charDelay)
    {
        sayText.gameObject.SetActive(true);
        sayText.text = "";
        skipType = false;

        int charIndex = 0;
        foreach (char c in text)
        {
            if (skipType)
            {
                sayText.text = text;
                break;
            }

            sayText.text += c;

            if (charIndex % 2 == 0)
            {
                G.audioSystem.Play(SoundId.SFX_Typing);
            }

            charIndex++;
            yield return new WaitForSeconds(charDelay);
        }

        yield return WaitForLeftClick();
        sayText.gameObject.SetActive(false);
    }

    private IEnumerator WaitForLeftClick()
    {
        yield return null;
        while (!Input.GetMouseButtonDown(0) && !Input.GetKeyDown(KeyCode.Space))
        {
            if (Input.GetMouseButton(0) || Input.GetKey(KeyCode.Space))
                skipType = true;
            yield return null;
        }
    }

    public void TogglePause(bool pause)
    {
        if (pauseManager == null) return;
        pauseManager.SetToggle(pause);
    }
}