using System;
using System.Collections.Generic;
using Mono.Cecil;
using UnityEngine;
using UnityEngine.Events;

public class Hand : MonoBehaviour, IDraggableOwner<DraggableCard>
{
    public List<CardInstance> cardsInHand = new();
    [SerializeField] private Transform cardsParent;

    private const float PLACEMENT_X_RANGE = 1.75f;
    
    [Header("Hover layout")]
    [SerializeField] private float hoveredBottomY = -3f; // локальный Y для наведённой карты
    [SerializeField] private float hoverSpread = 0.6f;
    [SerializeField] private float hoverSpreadFar = 0.3f;
    [SerializeField] private float layoutLerpSpeed = 12f;


    private CardInstance _hoveredCard;

    public void SetHovered(CardInstance card)
    {
        if (card == null) return;
        _hoveredCard = card;
    }

    public void ClearHovered(CardInstance card)
    {
        // очищаем только если очищает именно текущий hover
        if (_hoveredCard == card)
            _hoveredCard = null;
    }

    private void Awake()
    {
        G.Hand = this;
    }

    private void Update()
    {
        if (cardsInHand.Count > 0) UpdateCardsPositions();
    }

    // 1) Базовая раскладка веером
    private void GetBaseLayout(int index, out Vector3 pos, out Quaternion rot)
    {
        float t = (cardsInHand.Count == 1)
            ? 0.5f
            : (float)index / (cardsInHand.Count - 1);

        float xPos = Mathf.Lerp(-6f / 2f, 6f / 2f, t);
        float centerT = t - 0.5f;
        float yPos = 0.5f * (1f - Mathf.Abs(centerT) * 2f);
        float rotationZ = -15f * centerT * 2f;

        pos = new Vector3(xPos, yPos, 0f);
        rot = Quaternion.Euler(0f, 0f, rotationZ);
    }
    
    private void ApplyHoverOffset(int index, int hoveredIndex, ref Vector3 pos, ref Quaternion rot)
    {
        if (hoveredIndex == -1)
            return;

        int delta = index - hoveredIndex;

        // 1) раздвигаем соседей по X
        float extraX = 0f;

        if (delta == -1)       extraX -= hoverSpread;
        else if (delta == 1)   extraX += hoverSpread;
        else if (delta <= -2)  extraX -= hoverSpreadFar;
        else if (delta >= 2)   extraX += hoverSpreadFar;

        pos.x += extraX;

        // 2) если это наведённая карта — особая раскладка
        if (index == hoveredIndex)
        {
            // строго вертикально
            rot = Quaternion.identity;

            // по нижней линии (локальный Y руки)
            pos.y = hoveredBottomY;
        }
    }




    
    public void UpdateCardsPositions()
    {
        int hoveredIndex = -1;
        if (_hoveredCard != null)
        {
            hoveredIndex = cardsInHand.IndexOf(_hoveredCard);
        }

        for (int i = 0; i < cardsInHand.Count; i++)
        {
            var c = cardsInHand[i];
            if (c == null) continue;

            if (c.Draggable != null &&
                (c.Draggable.IsDragging || c.Draggable.IsReturning))
                continue;
            
            GetBaseLayout(i, out Vector3 targetPos, out Quaternion targetRot);
            
            ApplyHoverOffset(i, hoveredIndex, ref targetPos, ref targetRot);
            
            c.transform.localPosition = Vector3.Lerp(
                c.transform.localPosition,
                targetPos,
                layoutLerpSpeed * Time.deltaTime
            );

            c.transform.localRotation = Quaternion.Lerp(
                c.transform.localRotation,
                targetRot,
                layoutLerpSpeed * Time.deltaTime
            );
        }
    }



    public void Draw()
    {
        var allCards = CMS.GetAll<CardModel>();
        var cardModel = allCards.GetRandomElement();
        var cardInst = Instantiate(cardModel.Prefab, cardsParent, false);
        cardInst.SetModel(cardModel);
        cardInst.Draggable.SetOwner(this);
    }


    public void Clear()
    {
        for (int i = 0; i < cardsInHand.Count; i++)
        {
            var d = cardsInHand[i];
            if (d == null) continue;

            // G.PileManager.Discard(d.DiceDef);
            Destroy(d.gameObject);
            cardsInHand[i] = null;
        }

        cardsInHand.Clear();
    }

    public void OnDragEnter(DraggableCard d)
    {
        var card = d.instance;
        if (cardsInHand.Contains(card)) return;

        cardsInHand.Add(card);
        card.transform.SetParent(cardsParent, worldPositionStays: false);
    }

    public void OnDragExit(DraggableCard d)
    {
        var card = d.instance;
        cardsInHand.Remove(card);
    }
}