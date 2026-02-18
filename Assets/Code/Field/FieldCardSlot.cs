using System.Collections;
using TMPro;
using UnityEngine;

public class FieldCardSlot : ContainerBase<DraggableCard>, ITurnEntity
{
    // [field: SerializeField] public CardType acceptedType { get; private set; }
    public CardInstance AcceptedCard => GetAcceptedCard();

    [SerializeField] private TMP_Text turnIndexText;


    public override bool CanAccept(DraggableCard c)
    {
        if (!base.CanAccept(c)) return false;
        if (AcceptedDrag != null) return false;
        return true;
        // if (acceptedType == CardType.None) return true;
        // return c.instance.state.model.CardType == acceptedType;
    }

    public override void OnDragEnter(DraggableCard d)
    {
        base.OnDragEnter(d);
        d.SetLocked(true);
    }


    private CardInstance GetAcceptedCard()
    {
        if (AcceptedDrag == null) return null;
        return AcceptedDrag.instance;
    }

    public int TurnOrder { get; private set; }
    [field: SerializeField] public int Speed { get; private set; }


    public IEnumerator OnTurnEnd()
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