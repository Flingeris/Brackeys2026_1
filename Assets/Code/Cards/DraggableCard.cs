public class DraggableCard : DraggableWContainer<DraggableCard, ICardContainer>
{
    public bool IsLocked { get; private set; }


    public void SetLocked(bool isLocked)
    {
        this.IsLocked = isLocked;
    }
}