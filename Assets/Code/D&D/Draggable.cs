using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

public class Draggable : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    [Header("Return animation")] [SerializeField]
    private float returnDuration = 0.2f;

    [SerializeField] private Ease returnEase = Ease.OutCubic;

    public event UnityAction OnDragBegin;
    public event UnityAction OnDragEnd;


    [SerializeField] private SortingGroup sortGroup;
    public bool IsDragging { get; private set; } = false;
    public bool IsReturning { get; private set; }


    //protected bool isPickedUp = false;
    //protected static Draggable ActivePicked = null;

    private int origLayer;
    private int origSortOrder;
    protected Camera mainCamera;
    protected Vector3 offset;
    protected Vector3 origin;
    
    public static bool DisableHoverGlobal = false;
    public static bool DisableInteractionGlobal = false;


    public float HoverLockUntil { get; protected set; } = 0.2f;

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
        if (!CanDrag()) return;
        if (G.HUD != null && G.HUD.tooltip != null) G.HUD.tooltip.PushBlock();
        
        if (IsReturning)
        {
            transform.DOKill();
            IsReturning = false;
        }
        else
        {
            transform.DOKill();
        }
        
        G.currentDrag = this;
        IsDragging = true;

        origLayer = gameObject.layer;
        gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
        if (sortGroup != null)
        {
            origSortOrder = sortGroup.sortingOrder;
            sortGroup.sortingOrder = 9999;
        }

        origin = transform.position;

        var worldPos = mainCamera.ScreenToWorldPoint(eventData.position);
        worldPos.z = transform.position.z;
        offset = transform.position - worldPos;
        
        DisableHoverGlobal = true;
        OnDragBegin?.Invoke();
    }


    protected virtual bool CanDrag()
    {
        return !DisableInteractionGlobal;
    }

    public virtual void OnDrag(PointerEventData eventData)
    {
        if (!IsDragging) return;
        if (!CanDrag()) return;


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
        if (!IsDragging) return;
        if (!CanDrag()) return;
        LockHover();
        Release();
        
        DisableHoverGlobal = false;
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

    public void ReturnToOrigin()
    {
        transform.DOKill();

        IsReturning = true;

        transform.DOMove(origin, returnDuration)
            .SetEase(returnEase)
            .OnComplete(() => { IsReturning = false; });
    }


    public void LockHover(float seconds = 0.3f)
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