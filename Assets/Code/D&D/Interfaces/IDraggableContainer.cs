using System;
using UnityEngine.Events;

public interface IDraggableContainer<TDraggable> : IDraggableOwner<TDraggable>
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


public interface IDraggableOwner<TD>
    where TD : Draggable
{
    void OnDragEnter(TD d);
    void OnDragExit(TD d);
}