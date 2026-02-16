using UnityEngine.Events;

public interface IDraggableContainer<TDraggable>
    where TDraggable : Draggable
{
    event UnityAction OnContainerChanged;

    bool IsEmpty { get; }

    bool CanAccept(TDraggable d);

    bool CanRemove(TDraggable d);

    bool TryAccept(TDraggable d, out TDraggable oldD);

    bool TryRemove(TDraggable d);

    void Clear();
}

public interface ICardContainer : IDraggableContainer<DraggableCard>
{}
