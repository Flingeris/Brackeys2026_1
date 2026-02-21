using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class DraggableWContainer<TD, TC> : Draggable
    where TD : DraggableWContainer<TD, TC>
    where TC : class, IDraggableContainer<TD>
{
    public virtual TC CurrContainer { get; protected set; }
    public virtual IDraggableOwner<TD> Owner { get; protected set; }
    public Coroutine PutInContCoroutine { get; protected set; }
    private TD selfCasted => this as TD;

    public bool IsLocked { get; protected set; }


    protected override bool CanDrag()
    {
        if (!base.CanDrag()) return false;
        return !IsLocked;
    }

    public void SetLocked(bool isLocked)
    {
        IsLocked = isLocked;
    }


    public virtual void SetOwner(IDraggableOwner<TD> newOwner)
    {
        if (Owner == newOwner) return;

        var prev = Owner;

        prev?.OnDragExit(selfCasted);

        Owner = newOwner;

        if (newOwner is MonoBehaviour mb)
        {
            transform.SetParent(mb.transform, true);
        }

       
        Owner?.OnDragEnter(selfCasted);
    }

    protected virtual void SetContainer(TC newContainer)
    {
        CurrContainer = newContainer;

        if (newContainer is IDraggableOwner<TD> owner)
        {
            SetOwner(owner);
        }
    }


    protected override void OnDropped(PointerEventData eventData)
    {
        base.OnDropped(eventData);

        var rawContainer = DraggableUtil<TD>.FindContainer(selfCasted, eventData);
        var targetCont = rawContainer as TC;

        PutInContainer(targetCont);
    }

    public void PutInContainer(TC targetCont)
    {
        if (PutInContCoroutine != null)
        {
            StopCoroutine(PutInContCoroutine);
        }

        PutInContCoroutine = StartCoroutine(PutInContainerSequence(targetCont));
    }

    protected virtual IEnumerator PutInContainerSequence(TC targetCont)
    {
        var sourceCont = CurrContainer;

        if (!ValidateMove(sourceCont, targetCont))
        {
            ReturnToOrigin();
            PutInContCoroutine = null;
            yield break;
        }

        if (sourceCont != null && !ReferenceEquals(sourceCont, targetCont))
        {
            sourceCont.TryRemove(selfCasted);
        }

        if (!targetCont.TryAccept(selfCasted, out _))
        {
            ReturnToOrigin();
            PutInContCoroutine = null;
            yield break;
        }

        SetContainer(targetCont);

        PutInContCoroutine = null;
        yield break;
    }


    protected virtual bool ValidateMove(TC sourceCont, TC targetCont)
    {
        if (targetCont == null) return false;
        if (sourceCont != null && !sourceCont.CanRemove(selfCasted)) return false;
        if (!targetCont.CanAccept(selfCasted)) return false;
        return true;
    }
}