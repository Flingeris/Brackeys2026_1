using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class CardState
{
    public CardModel model;
}

public class CardInstance : MonoBehaviour
{
    public CardState state;
    public DraggableCard Draggable;

    [SerializeField] private TMP_Text cardNameText;
    [SerializeField] private TMP_Text cardDescText;
    [SerializeField] private TMP_Text cardClassText;
    [SerializeField] private TMP_Text cardTypeText;


    public void SetState(CardState newState)
    {
        this.state = newState;
        UpdateVisuals();
    }

    public void SetModel(CardModel model)
    {
        if (model == null) return;

        var newState = new CardState()
        {
            model = model
        };

        SetState(newState);
    }

    private void UpdateVisuals()
    {
        UpdateClassText();
        UpdateTypeText();
    }

    private void UpdateClassText()
    {
        cardClassText.text = state.model.ClassType.ToString()[0].ToString();
    }

    private void UpdateTypeText()
    {
        cardTypeText.text = state.model.CardType.ToString()[0].ToString();
    }

    public IEnumerator OnTurnEndSequence()
    {
        var startPos = transform.position;
        var NewPos = startPos;
        NewPos.y += 0.8f;
        yield return transform.DOMove(NewPos, 0.2f).WaitForCompletion();
        transform.DOShakePosition(0.5f, 0.2f, 40);

        var logic = state.model.CardLogic;
        foreach (var e in logic)
        {
            e.Use();
        }

        yield return new WaitForSeconds(0.5f);
        yield return transform.DOMove(startPos, 0.2f).WaitForCompletion();
    }
}