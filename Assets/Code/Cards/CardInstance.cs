using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

public class CardInstance : MonoBehaviour
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
        var ownerTransform = state.CardOwner.spriteRenderer.transform;
        var ownerStartPos = ownerTransform.position;
        var ownerEndPos = ownerStartPos;
        ownerEndPos.x += 3f;

        var startPos = transform.position;
        var NewPos = startPos;
        NewPos.y += 0.3f;
        yield return transform.DOMove(NewPos, 0.2f).WaitForCompletion();
        transform.DOShakePosition(0.5f, 0.2f, 40);
        yield return new WaitForSeconds(0.5f);
        
        
        var inters = state.model.OnTurnEndInteractions;
        foreach (var i in inters)
        {
            yield return i.OnEndTurn(state);
            if (i is not INoAnimationAction)
            {
                ownerTransform.DOMove(ownerEndPos, 0.2f);
                yield return new WaitForSeconds(0.2f);
                ownerTransform.DOMove(ownerStartPos, 0.2f);
                yield return new WaitForSeconds(0.2f);
            }
        }


 
        yield return transform.DOMove(startPos, 0.2f).WaitForCompletion();
    }


    public void Leave()
    {
        Draggable.Leave();
    }
}