using System.Linq;
using UnityEngine;

public class Field : MonoBehaviour
{
    public FieldCardSlot[] cardsSlots;

    public CardInstance[] PlayedCards => cardsSlots.Select(r => r.AcceptedCard).ToArray();

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


    public void Clear()
    {
        foreach (var c in cardsSlots)
        {
            if (c == null) continue;
            c.Clear();
        }
    }
}