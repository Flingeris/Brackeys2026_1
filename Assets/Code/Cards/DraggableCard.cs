using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableCard
    : DraggableWContainer<DraggableCard, IDraggableContainer<DraggableCard>>
{
    public CardInstance instance;

    [Header("Drag physics")]
    [SerializeField] private float dragMaxSpeed = 20f;
    [SerializeField] private float dragAccelLerp = 25f;
    [SerializeField] private float dragTiltAmount = 1f;
    [SerializeField] private float dragTiltReturnSpeed = 10f;
    [SerializeField] float spring = 60f;
    [SerializeField] float damping = 10f;  // сопротивление, чтобы не разносило 

    [Header("Visual")]
    [SerializeField] private Transform visual;

    private Vector3 dragTargetPos;
    private Vector3 dragVelocity;
    private float currentTilt;
    private Quaternion baseVisualLocalRot;
    private bool wasInContainerOnDragStart;   
    private IDraggableContainer<DraggableCard> dragSourceContainer;
    

    private void Awake()
    {
        if (visual == null)
        {
            var t = transform.Find("Visual");
            if (t != null) visual = t;
        }

        if (visual != null)
            baseVisualLocalRot = visual.localRotation;
    }

    public override void OnBeginDrag(PointerEventData eventData)
    {

        dragSourceContainer = CurrContainer;
        if (CurrContainer != null)
        {
            CurrContainer.TryRemove(this);
            SetContainer(null);
        }
        
        base.OnBeginDrag(eventData);

        var cursorWorldPos = mainCamera.ScreenToWorldPoint(eventData.position);
        cursorWorldPos.z = transform.position.z;
        dragTargetPos = cursorWorldPos + offset;

        dragVelocity = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }



    protected override void UpdatePosToCursor()
    {
        var cursorWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        cursorWorldPos.z = transform.position.z;
        dragTargetPos = cursorWorldPos + offset;
    }

    private void Update()
    {
        if (!IsDragging) return;

        UpdateDragPhysics();
    }


    private void UpdateDragPhysics()
    {
        Vector3 toTarget = dragTargetPos - transform.position;

        // F = kx - bv
        Vector3 accel = toTarget * spring - dragVelocity * damping;
        dragVelocity += accel * Time.deltaTime;

        transform.position += dragVelocity * Time.deltaTime;

        float targetTilt = Mathf.Clamp(-dragVelocity.x * dragTiltAmount, -80f, 80f);
        currentTilt = Mathf.Lerp(currentTilt, targetTilt, dragTiltReturnSpeed * Time.deltaTime);

        if (visual != null)
            visual.localRotation = baseVisualLocalRot * Quaternion.Euler(0f, 0f, currentTilt);

    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        base.OnEndDrag(eventData);
        dragVelocity = Vector3.zero;
        currentTilt = 0f;

        if (visual != null)
            visual.localRotation = baseVisualLocalRot;
    }

    protected override System.Collections.IEnumerator PutInContainerSequence(IDraggableContainer<DraggableCard> targetCont)
    {
        bool fromSlot = dragSourceContainer != null;
        
        if (targetCont == null)
        {
            if (fromSlot)
            {
                SetOwner(G.Hand);
            }
            else
            {
                ReturnToOrigin();
            }

            PutInContCoroutine = null;
            dragSourceContainer = null;
            yield break;
        }
        
        if (!targetCont.CanAccept(this))
        {
            if (fromSlot)
                SetOwner(G.Hand);
            else
                ReturnToOrigin();

            PutInContCoroutine = null;
            dragSourceContainer = null;
            yield break;
        }
        
        if (!targetCont.TryAccept(this, out var oldCard))
        {
            if (fromSlot)
                SetOwner(G.Hand);
            else
                ReturnToOrigin();

            PutInContCoroutine = null;
            dragSourceContainer = null;
            yield break;
        }
        
        SetContainer(targetCont);
        
        if (oldCard != null && oldCard != this)
        {
            if (dragSourceContainer != null &&
                dragSourceContainer.CanAccept(oldCard))
            {
                dragSourceContainer.TryAccept(oldCard, out _);
                oldCard.SetContainer(dragSourceContainer);
            }
            else
            {
                oldCard.SetOwner(G.Hand);
            }
        }

        PutInContCoroutine = null;
        dragSourceContainer = null;
    }
}
