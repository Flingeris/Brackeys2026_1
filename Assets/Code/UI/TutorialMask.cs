using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class TutorialMask : MonoBehaviour
{
    public TMP_Text TutorialText;
    public RectTransform mask;

    [SerializeField] private RectTransform hand;
    [SerializeField] private RectTransform fieldSlots;
    [SerializeField] private RectTransform numbers;
    [SerializeField] private RectTransform endTurn;
    [SerializeField] private RectTransform characters;

    bool skip;

    private void Awake()
    {
        Hide();
    }

    public void SetTutorialText(string text, int posY = 0, int posX = 0)
    {
        TutorialText.text = text;
        TutorialText.rectTransform.anchoredPosition = new Vector2(posX, posY);
    }


    public void ShowHand()
    {
        Show(hand);
    }

    public void ShowField()
    {
        Show(fieldSlots);
    }

    public void ShowNumbers()
    {
        Show(numbers);
    }

    public void ShowEndTurn()
    {
        Show(endTurn);
    }

    public void ShowCharacters()
    {
        Show(characters);
    }


    public void Show(RectTransform at)
    {
        gameObject.SetActive(true);

        var rect = at.rect;

        mask.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, rect.width);
        mask.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, rect.height);
        mask.anchoredPosition = at.anchoredPosition;
    }

    public IEnumerator WaitForSkip()
    {
        yield return new WaitForSeconds(0.5f);
        skip = false;

        while (!skip)
        {
            yield return new WaitForEndOfFrame();
        }

        Hide();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            skip = true;
        }
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}