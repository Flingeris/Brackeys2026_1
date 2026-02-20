using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class CardState
{
    public CardModel model;
    public PartyMember CardOwner => G.party.GetMemberByClass(model.ClassType);

    public CardState(CardModel model)
    {
        this.model = model;
    }
}

public class CardInstance : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public CardState state;
    [Header("References")] public DraggableCard Draggable;
    public Hand Hand => G.Hand;


    [SerializeField] private TMP_Text cardNameText;
    [SerializeField] private TMP_Text cardDescText;
    [SerializeField] private TMP_Text cardClassText;
    [SerializeField] private TMP_Text turnIndexText;

    [SerializeField] private SpriteRenderer cardSprite;


    public void SetState(CardState newState)
    {
        this.state = newState;
        UpdateVisuals();
    }

    public void SetModel(CardModel model)
    {
        if (model == null) return;

        var newState = new CardState(model);

        SetState(newState);
    }

    private void UpdateVisuals()
    {
        UpdateClassText();
        UpdateSprite();
        UpdateName();
        UpdateDescription();
    }

    private void UpdateName()
    {
        cardNameText.text = state.model.name;
    }

    private void UpdateDescription()
    {
        cardDescText.text = state.model.Description;
    }


    private void UpdateClassText()
    {
        cardClassText.text = state.model.ClassType.ToString()[0].ToString();
    }


    private void UpdateSprite()
    {
        cardSprite.sprite = state.model.Sprite;
    }


    public IEnumerator OnCardPlayed()
    {
        var inters = state.model.OnPlayedCardInteractions;
        foreach (var i in inters)
        {
            yield return i.OnPlay(state);
        }
    }


    public IEnumerator OnTurnEnd()
    {
        var startPos = transform.position;
        var NewPos = startPos;
        NewPos.y += 0.8f;
        yield return transform.DOMove(NewPos, 0.2f).WaitForCompletion();

        var inters = state.model.OnTurnEndInteractions;
        foreach (var i in inters)
        {
            yield return i.OnEndTurn(state);
        }

        transform.DOShakePosition(0.5f, 0.2f, 40);


        yield return new WaitForSeconds(0.5f);
        yield return transform.DOMove(startPos, 0.2f).WaitForCompletion();
    }


    public void OnPointerEnter(PointerEventData eventData)
    {
        if (G.main.IsChoosingTarget) return;
        List<ICardInteraction> inters = new List<ICardInteraction>();
        inters.AddRange(state.model.OnPlayedCardInteractions);
        inters.AddRange(state.model.OnTurnEndInteractions);
        bool isTarget = false;
        foreach (var i in inters)
        {
            if (i is ChooseTargetInteractionBase targetInteraction)
            {
                isTarget = true;
            }
        }
    }

    public void Leave()
    {
        this.Draggable.Leave();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (G.main.IsChoosingTarget) return;
    }
}