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

    private void Awake()
    {
        if (visual == null)
        {
            var t = transform.Find("Visual");
            if (t != null) visual = t;
        }
    }

    public override void OnBeginDrag(PointerEventData eventData)
    {
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
            visual.localRotation = Quaternion.Euler(0f, 0f, currentTilt);
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        base.OnEndDrag(eventData);
        dragVelocity = Vector3.zero;
        currentTilt = 0f;
        if (visual != null)
        {
            visual.localRotation = Quaternion.identity;
        }
    }
}
