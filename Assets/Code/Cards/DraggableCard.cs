using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableCard
    : DraggableWContainer<DraggableCard, IDraggableContainer<DraggableCard>>
{
    public CardInstance instance;

    [Header("Drag physics")]

    [SerializeField] private float dragTiltAmount = 1f;
    [SerializeField] private float dragTiltReturnSpeed = 10f;
    [SerializeField] float spring = 60f;
    [SerializeField] float damping = 10f; // сопротивление, чтобы не разносило 

    [Header("Visual")] [SerializeField] private Transform visual;

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


    public void Leave()
    {
        SetOwner(null);
    }

    public override void OnBeginDrag(PointerEventData eventData)
    {
        base.OnBeginDrag(eventData);
        if(!CanDrag()) return;
float pitch = Random.Range(0.96f, 1.04f);
        G.audioSystem.PlayPitched(SoundId.SFX_CardGrab, pitch);
        
        if (visual != null)
            visual.DOKill();

        dragSourceContainer = CurrContainer;

        if (CurrContainer != null)
        {
            CurrContainer.TryRemove(this);
            SetContainer(null);
        }

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
        // currentTilt = 0f;

        // if (visual != null)
        // visual.localRotation = baseVisualLocalRot;
    }

    protected override IEnumerator PutInContainerSequence(IDraggableContainer<DraggableCard> targetCont)
    {
        var sourceCont = dragSourceContainer;
        bool fromSlot = sourceCont != null;

        if (targetCont == null)
        {
            if (fromSlot)
            {
                SetContainer(null);
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

        if (fromSlot && targetCont == sourceCont)
        {
            if (!targetCont.TryAccept(this, out _))
            {
                SetOwner(G.Hand);
            }
            else
            {
                SetContainer(targetCont);
            }

            PutInContCoroutine = null;
            dragSourceContainer = null;
            yield break;
        }

        if (!targetCont.TryAccept(this, out var oldCard))
        {
            if (fromSlot)
            {
                SetContainer(null);
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

        SetContainer(targetCont);

        if (oldCard != null && oldCard != this)
        {
            if (fromSlot && sourceCont != null && sourceCont.CanAccept(oldCard))
            {
                sourceCont.TryAccept(oldCard, out _);
                oldCard.SetContainer(sourceCont);
            }
            else
            {
                oldCard.SetContainer(null);
                oldCard.SetOwner(G.Hand);
            }
    float pitch = 1f;
        G.audioSystem.PlayPitched(SoundId.SFX_CardSwap, pitch);
    }

        PutInContCoroutine = null;
        dragSourceContainer = null;
    }
}