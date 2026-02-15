using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class DraggableWContainer<TDraggable, TContainer> : Draggable
    where TDraggable : DraggableWContainer<TDraggable, TContainer>
    where TContainer : class, IDraggableContainer<TDraggable>
{
    public virtual TContainer CurrContainer { get; protected set; }
    public Coroutine PutInContCoroutine { get; protected set; }
    private TDraggable selfCasted => this as TDraggable;

    public virtual void SetContainer(TContainer newContainer)
    {
        CurrContainer = newContainer;
    }

    protected override void OnDropped(PointerEventData eventData)
    {
        base.OnDropped(eventData);

        var raw = DraggableUtil<TDraggable>.FindContainer(selfCasted, eventData);
        var targetCont = raw as TContainer;
        PutInContainer(targetCont);
    }

    public void PutInContainer(TContainer targetCont)
    {
        if (PutInContCoroutine != null)
        {
            StopCoroutine(PutInContCoroutine);
        }
        PutInContCoroutine = StartCoroutine(PutInContainerCoroutine(targetCont));   
    }

    protected virtual IEnumerator PutInContainerCoroutine(TContainer targetCont)
    {
        var sourceCont = CurrContainer;
        if (targetCont != null && targetCont.TryAccept(selfCasted, out var oldD))
        {
            if (sourceCont != null && !ReferenceEquals(sourceCont, targetCont))
            {
                sourceCont.TryRemove(selfCasted);
                if (oldD != null)
                {
                    oldD.LockHover();
                    sourceCont.TryAccept(oldD, out _);
                }
            }
        }
        else
        {
            ReturnToOrigin();
            yield break;
        }

        PutInContCoroutine = null;
    }
}