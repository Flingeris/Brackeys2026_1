using UnityEngine;

public class FieldCardSlot : ContainerBase<CardInstance>, ICardContainer
{
    [SerializeField] private CardType acceptedType;


    public override bool CanAccept(CardInstance c)
    {
        if(!base.CanAccept(c)) return false;
        if(acceptedType == CardType.None) return true;
        return c.state.model.CardType == acceptedType;
    }
}