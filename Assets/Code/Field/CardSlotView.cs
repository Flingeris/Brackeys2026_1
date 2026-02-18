using System;
using TMPro;
using UnityEngine;

public class CardSlotView : ContainerViewBase<DraggableCard, FieldCardSlot>
{
    [SerializeField] private TMP_Text slotTypeText;

    private void Start()
    {
        // UpdateText();
    }

    // private void UpdateText()
    // {
    //     slotTypeText.text = containerInst.acceptedType.ToShortName();
    // }
}