using System.Linq;
using UnityEngine;

public class Field : MonoBehaviour
{
    [SerializeField] private FieldCardSlot[] cardsSlots;

    public CardInstance[] PlayedCards => cardsSlots.Select(r => r.AcceptedDrag).ToArray();

    private void OnValidate()
    {
        if (cardsSlots == null || cardsSlots.Length == 0 || cardsSlots.Length != transform.childCount)
            cardsSlots = GetComponentsInChildren<FieldCardSlot>();
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