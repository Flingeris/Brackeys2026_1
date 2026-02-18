using UnityEngine;

public class FieldCardSlot : ContainerBase<DraggableCard>
{
    [SerializeField] private CardType acceptedType;
    public CardInstance AcceptedCard => GetAcceptedCard();


    public override bool CanAccept(DraggableCard c)
    {
        if (!base.CanAccept(c)) return false;
        if (acceptedType == CardType.None) return true;
        return c.instance.state.model.CardType == acceptedType;
    }



    public override void OnDragEnter(DraggableCard d)
    {
        base.OnDragEnter(d);

    }

    public override void OnDragExit(DraggableCard d)
    {
        base.OnDragExit(d);
        d.SetLocked(false);
    }


    private CardInstance GetAcceptedCard()
    {
        if (AcceptedDrag == null) return null;
        return AcceptedDrag.instance;
    }
}