using System.Collections;
using TMPro;
using UnityEngine;

public class FieldCardSlot : ContainerBase<DraggableCard>, ITurnEntity
{
    public CardInstance AcceptedCard => GetAcceptedCard();
    

    [SerializeField] private TMP_Text turnIndexText;


    private CardInstance GetAcceptedCard()
    {
        if (AcceptedDrag == null) return null;
        return AcceptedDrag.instance;
    }

    public int TurnOrder { get; private set; }
    [field: SerializeField] public int Speed { get; private set; }


    public IEnumerator OnTurn()
    {
        if (AcceptedCard != null) yield return AcceptedCard.OnTurnEnd();
    }

    private void UpdateTurnIndex()
    {
        turnIndexText.text = "";
        if (TurnOrder > 0)
            turnIndexText.text = TurnOrder.ToString();
    }


    public void SetTurnIndex(int turnIndex)
    {
        TurnOrder = turnIndex;
        UpdateTurnIndex();
    }
}