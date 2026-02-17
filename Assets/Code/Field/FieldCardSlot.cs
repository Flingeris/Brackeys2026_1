using UnityEngine;

public class FieldCardSlot : ContainerBase<DraggableCard>
{
    [field: SerializeField] public CardType acceptedType { get; private set; }
    public CardInstance AcceptedCard => GetAcceptedCard();


    public override bool CanAccept(DraggableCard c)
    {
        if (!base.CanAccept(c)) return false;
        if (AcceptedDrag != null) return false;
        if (acceptedType == CardType.None) return true;
        return c.instance.state.model.CardType == acceptedType;
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
}