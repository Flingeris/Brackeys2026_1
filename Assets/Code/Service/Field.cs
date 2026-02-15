using UnityEngine;

public class Field : MonoBehaviour
{
    [SerializeField] private FieldCardSlot[] cardsSlots;


    private void OnValidate()
    {
        if(cardsSlots == null ||  cardsSlots.Length == 0 || cardsSlots.Length != transform.childCount) 
            cardsSlots = GetComponentsInChildren<FieldCardSlot>();
    }
}