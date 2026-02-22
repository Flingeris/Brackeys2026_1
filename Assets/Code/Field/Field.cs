using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using TMPro;
using UnityEngine;

public class Field : MonoBehaviour
{
    public FieldCardSlot[] cardsSlots;

    public CardInstance[] PlayedCards =>
        cardsSlots.Where(r => r.AcceptedCard != null).Select(r => r.AcceptedCard).ToArray();

    public List<TMP_Text> turnOrderNumbers;

    private void OnValidate()
    {
        if (cardsSlots == null || cardsSlots.Length == 0 || cardsSlots.Length != transform.childCount)
            cardsSlots = GetComponentsInChildren<FieldCardSlot>();
    }

    public int CountCardsWithClass(ClassType classType)
    {
        var count = 0;
        foreach (var card in PlayedCards)
        {
            if (card == null) continue;
            if (card.state.model.ClassType == classType)
            {
                count++;
            }
        }

        return count;
    }


    public IEnumerator Clear()
    {
        foreach (var cardsSlot in cardsSlots)
        {
            if (cardsSlot == null) continue;
            if (cardsSlot.AcceptedCard != null) yield return G.main.KillCard(cardsSlot.AcceptedCard);
        }
    }

    // public void Clear()
    // {
    //     foreach (var c in cardsSlots)
    //     {
    //         if (c == null) continue;
    //         c.Clear();
    //     }
    // }
}