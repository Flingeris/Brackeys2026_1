using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public static class DraggableUtil<TDraggable>
    where TDraggable : Draggable
{
    public static IDraggableContainer<TDraggable> FindContainer(TDraggable draggable, PointerEventData eventData = null)
    {
        if (eventData != null)
        {
            var uiCont = FindContainerUI(eventData, draggable);
            if (uiCont != null)
            {
                return uiCont;
            }
        }

        return FindContainerPhysics(draggable);
    }

    private static IDraggableContainer<TDraggable> FindContainerUI(PointerEventData eventData, TDraggable draggable)
    {
        if (EventSystem.current == null) return null;

        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (var r in results)
        {
            if (r.gameObject == null) continue;
            if (!r.gameObject.TryGetComponent<IDraggableContainer<TDraggable>>(out var cont)) continue;
            if (cont == draggable) continue;

            if (!cont.CanAccept(draggable)) continue;

            return cont;
        }

        return null;
    }

    private static IDraggableContainer<TDraggable> FindContainerPhysics(TDraggable draggable)
    {
        var cols = Physics2D.OverlapCircleAll(draggable.transform.position, 0.5f);

        IDraggableContainer<TDraggable> best = null;
        float bestDist = float.MaxValue;

        foreach (var overlapCol in cols)
        {
            if (!overlapCol.TryGetComponent<IDraggableContainer<TDraggable>>(out var dragContainer)) continue;
            if (overlapCol.gameObject == draggable.gameObject) continue;

            if (dragContainer is not MonoBehaviour acceptorMB) continue;

            float dist = Vector3.Distance(draggable.transform.position, acceptorMB.transform.position);
            if (dist < bestDist)
            {
                bestDist = dist;
                best = dragContainer;
            }
        }

        return best;
    }
}