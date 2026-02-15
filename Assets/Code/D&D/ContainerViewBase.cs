using UnityEngine;

public abstract class ContainerViewBase<TDraggable, TContainer> : MonoBehaviour
    where TDraggable : Draggable
    where TContainer : ContainerBase<TDraggable>
{
    [Header("Base Container view References")]
    [SerializeField] protected SpriteRenderer greenHighlight;
    [SerializeField] protected TContainer containerInst;

    protected virtual void OnValidate()
    {
        if (containerInst == null) containerInst = GetComponent<TContainer>();
    }

    protected virtual void SetGreen(bool v)
    {
        if (greenHighlight == null) return;

        if (v == true && containerInst.CanAccept(G.currentDrag))
        {
            greenHighlight.enabled = true;
        }
        else
        {
            greenHighlight.enabled = false;
        }
    }

    protected virtual void Update()
    {
        SetGreen(G.currentDrag != null);
    }
}