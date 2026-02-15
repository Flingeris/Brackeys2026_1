using System;
using System.Collections.Generic;
using UnityEngine;

public class Hand : MonoBehaviour
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
            if(c.IsDragging) continue;
            
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
            c.transform.localPosition =new Vector3(xPos, yPos, 0f);
            c.transform.localRotation = Quaternion.Euler(0f, 0f, rotationZ);
        }
    }

    public void Draw()
        {
            var cardModel = CMS.Get<CardModel>("card0");
            var card = Instantiate(cardModel.Prefab, cardsParent, false);
            if (card == null) return;
            cardsInHand.Add(card);
            UpdateCardsPositions();
        }
    }