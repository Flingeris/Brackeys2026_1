using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;

public class Draggable : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    public event UnityAction OnDragBegin;

    public event UnityAction OnDragEnd;

    [SerializeField] private SortingGroup sortGroup;
    public bool IsDragging { get; private set; } = false;

    //protected bool isPickedUp = false;
    //protected static Draggable ActivePicked = null;

    private int origLayer;
    private int origSortOrder;
    private Camera mainCamera;
    private Vector3 offset;
    private Vector3 origin;

    protected float HoverLockUntil = 0.2f;

    private void OnValidate()
    {
        if (sortGroup == null) sortGroup = GetComponent<SortingGroup>();
    }

    private void Start()
    {
        mainCamera = Camera.main;
    }

    public virtual void OnBeginDrag(PointerEventData eventData)
    {
        if (G.HUD != null && G.HUD.tooltip != null) G.HUD.tooltip.PushBlock();

        G.currentDrag = this;
        IsDragging = true;

        origLayer = gameObject.layer;
        gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
        if (sortGroup != null)
        {
            origSortOrder = sortGroup.sortingOrder;
            sortGroup.sortingOrder = 1000;
        }

        origin = transform.position;

        var worldPos = mainCamera.ScreenToWorldPoint(eventData.position);
        worldPos.z = transform.position.z;
        offset = transform.position - worldPos;

        OnDragBegin?.Invoke();
    }

    public virtual void OnDrag(PointerEventData eventData)
    {
        UpdatePosToCursor();
    }

    protected virtual void UpdatePosToCursor()
    {
        var cursorWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        cursorWorldPos.z = transform.position.z;
        transform.position = cursorWorldPos + offset;
    }

    public virtual void OnEndDrag(PointerEventData eventData)
    {
        LockHover();
        Release();

        OnDropped(eventData);
    }

    protected virtual void Release()
    {
        G.currentDrag = null;
        IsDragging = false;

        gameObject.layer = origLayer;
        if (sortGroup != null) sortGroup.sortingOrder = origSortOrder;

        if (G.HUD != null && G.HUD.tooltip != null) G.HUD.tooltip.PopBlock();

        OnDragEnd?.Invoke();
    }

    protected virtual void OnDropped(PointerEventData eventData)
    {
    }

    protected virtual void ReturnToOrigin()
    {
        transform.DOMove(origin, 0.05f).SetId("Draggable return to origin");
    }

    public void LockHover(float seconds = 0.5f)
    {
        HoverLockUntil = Mathf.Max(HoverLockUntil, Time.time + seconds);
    }

    protected virtual void OnDestroy()
    {
        transform.DOKill();
    }

    public virtual void OnPointerClick(PointerEventData eventData)
    {
        //if (eventData.dragging) return;
        //if (ActivePicked != null && ActivePicked != this) return;

        //if (!isPickedUp)
        //{
        //    OnBeginDrag(eventData);
        //    isPickedUp = true;
        //    ActivePicked = this;
        //    eventData.Use();
        //    Debug.Log("Begin drag with click " + isPickedUp + " " + ActivePicked.name);
        //}
        //else
        //{
        //    OnEndDrag(eventData);
        //    isPickedUp = false;
        //    ActivePicked = null;
        //    eventData.Use();
        //}
    }

    //private void Update()
    //{
    //    if (isPickedUp)
    //    {
    //        UpdatePosToCursor();
    //    }
    //}

    //private void OnDisable()
    //{
    //    if (ActivePicked == this) ActivePicked = null;
    //    isPickedUp = false;
    //    IsDragging = false;
    //}
}