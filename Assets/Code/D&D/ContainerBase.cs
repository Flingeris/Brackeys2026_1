using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

public abstract class ContainerBase<TDraggable> : MonoBehaviour, IDraggableContainer<TDraggable>
    where TDraggable : Draggable
{
    public event UnityAction OnContainerChanged;

    [field: SerializeField] public TDraggable AcceptedDrag { get; protected set; }
    public bool IsLocking;

    public bool IsEmpty => AcceptedDrag == null;
    [SerializeField] protected bool AnimDragOnAccept = true;

    public virtual bool TryAccept(TDraggable d, out TDraggable oldD)
    {
        oldD = AcceptedDrag;

        if (!CanAccept(d)) return false;

        AcceptedDrag = d;
        //d.transform.SetParent(this.transform, worldPositionStays: true);
        if (gameObject.activeInHierarchy && AnimDragOnAccept)
        {
            d.transform.DOMove(transform.position, 0.15f).SetId("Container moving draggable to centre");
            d.transform.DORotate(transform.rotation.eulerAngles, 0.15f);
        }
        else
        {
            d.transform.position = transform.position;
            d.transform.rotation = transform.rotation;
        }

        OnContainerChanged?.Invoke();
        return true;
    }

    public virtual bool TryRemove(TDraggable d)
    {
        if (!CanRemove(d)) return false;
        AcceptedDrag = null;

        OnContainerChanged?.Invoke();
        return true;
    }

    public virtual bool CanAccept(TDraggable d)
    {
        return d != null;
    }

    public bool CanAccept(MonoBehaviour d)
    {
        if (d == null) return false;
        var di = d.GetComponent<TDraggable>();
        return CanAccept(di);
    }

    public virtual bool CanRemove(TDraggable d)
    {
        return AcceptedDrag == d;
        // if (AcceptedDrag != d) return false;
        // return IsLocking;


    }

    public virtual void Clear()
    {
        if (AcceptedDrag == null) return;
        var d = AcceptedDrag;
        if (!TryRemove(AcceptedDrag)) return;

        OnClear(d);

        Destroy(d.gameObject);
    }

    protected virtual void OnClear(TDraggable d)
    {
    }
}