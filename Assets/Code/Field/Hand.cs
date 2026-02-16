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

    private void Awake()
    {
        G.Hand = this;
    }

    private void Update()
    {
        if (cardsInHand.Count > 0) UpdateCardsPositions();
    }

    private void UpdateCardsPositions()
    {
        if (cardsInHand.Count == 0) return;

        for (int i = 0; i < cardsInHand.Count; i++)
        {
            CardInstance c = cardsInHand[i];

            if (c == null) continue;
            if (c.Draggable.IsDragging) continue;

            float t = (cardsInHand.Count == 1)
                ? 0.5f
                : (float)i / (cardsInHand.Count - 1);

            float xPos = Mathf.Lerp(-6f / 2f, 6f / 2f, t);

            float centerT = t - 0.5f;

            float yPos = (0.5f * (1f - Mathf.Abs(centerT) * 2f));

            float rotationZ = -15f * centerT * 2f;

            // if (Cur.instance != null && Cursor3D.instance.CurrentInteractable == c)
            // {
            //     yPos += 0.3f;
            // }

            // c.InHandPos = new Vector3(xPos, yPos, 0f);
            c.transform.localPosition = new Vector3(xPos, yPos, 0f);
            c.transform.localRotation = Quaternion.Euler(0f, 0f, rotationZ);
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