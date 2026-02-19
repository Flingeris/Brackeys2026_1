using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using UnityEngine.Rendering;

public class CardHoverVFX : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Hand hover")] [SerializeField]
    private float handHoverScale = 1.7f;

    [Header("Slot hover")] [SerializeField]
    private float slotHoverScale = 1.5f;

    [SerializeField] private float slotHoverLift = 0.2f;
    [SerializeField] private float slotHoverTiltX = -10f;
    [SerializeField] private float slotHoverAnimDuration = 0.12f;

    [Header("Common")] [SerializeField] private float scaleDuration = 0.12f;
    [SerializeField] private Ease scaleEase = Ease.OutBack;
    [SerializeField] private Transform visual; // ССЫЛКА НА ВИЗУАЛ КАРТЫ

    [Header("Sorting / Front")] [SerializeField]
    private int hoverSortingBoost = 200;

    [SerializeField] private bool bringToFrontInUIIfNoSortingGroup = true;

    private CardInstance _card;
    private Draggable _draggable;
    private DraggableCard _draggableCard;

    private SortingGroup _sortingGroup;
    private int _baseSortingOrder;
    private Transform _baseParent;
    private int _baseSiblingIndex;

    private bool _isDragging;

    private Vector3 _baseScale;
    private Vector3 _visualBaseLocalPos;
    private Quaternion _visualBaseLocalRot;

    private Tween _scaleTween;
    private Tween _slotMoveTween;
    private Tween _slotRotateTween;


    private void Awake()
    {
        _card = GetComponent<CardInstance>() ?? GetComponentInParent<CardInstance>();
        _draggable = GetComponentInParent<Draggable>();
        _draggableCard = GetComponentInParent<DraggableCard>();

        if (_draggable != null)
        {
            _draggable.OnDragBegin += HandleDragBegin;
            _draggable.OnDragEnd += HandleDragEnd;
        }

        if (visual == null)
        {
            var t = transform.Find("Visual");
            visual = t != null ? t : transform;
        }

        _baseScale = visual.localScale;
        _visualBaseLocalPos = visual.localPosition;
        _visualBaseLocalRot = visual.localRotation;

        _sortingGroup = GetComponent<SortingGroup>();
        if (_sortingGroup != null)
            _baseSortingOrder = _sortingGroup.sortingOrder;

        _baseParent = transform.parent;
        _baseSiblingIndex = transform.GetSiblingIndex();
    }


    private bool IsInSlot()
    {
        return _draggableCard != null &&
               _draggableCard.CurrContainer != null;
    }


    private void HandleDragBegin()
    {
        _isDragging = true;
        
        _card?.Hand?.ClearHovered(_card);
        
        _scaleTween?.Kill();
        _slotMoveTween?.Kill();
        _slotRotateTween?.Kill();
        
        visual.localPosition = _visualBaseLocalPos;
        visual.localRotation = _visualBaseLocalRot;
        

    }


    private void HandleDragEnd()
    {
        _isDragging = false;
        
        _scaleTween?.Kill();
        _slotMoveTween?.Kill();
        _slotRotateTween?.Kill();
        visual.DOKill();
        
        visual.localScale = _baseScale;
        visual.localPosition = _visualBaseLocalPos;
        visual.localRotation = _visualBaseLocalRot;
        
        RestoreOrder();
    }


    public void OnPointerEnter(PointerEventData eventData)
    {
        if (Draggable.DisableHoverGlobal)
            return;
        
        if (G.currentDrag != null)
            return;
        
        
        if (Time.time < _draggable.HoverLockUntil) return;
        if (_isDragging) return;

        if (IsInSlot())
        {
            visual.DOKill();
            PlaySlotHoverScale(true);


            _slotMoveTween = visual.DOLocalMove(
                    _visualBaseLocalPos + new Vector3(0f, slotHoverLift, 0f),
                    slotHoverAnimDuration)
                .SetEase(Ease.OutCubic);

            _slotRotateTween = visual.DOLocalRotateQuaternion(
                    Quaternion.Euler(slotHoverTiltX, 0f, 0f) * _visualBaseLocalRot,
                    slotHoverAnimDuration)
                .SetEase(Ease.OutCubic);

            BringToFront();
            return;
        }
        
        PlayHandHoverScale(true);
        BringToFront();
        _card?.Hand?.SetHovered(_card);
    }


    public void OnPointerExit(PointerEventData eventData)
    {
        if (Draggable.DisableHoverGlobal)
            return;
        
        if (Time.time < _draggable.HoverLockUntil) return;
        if (_isDragging) return;

        if (IsInSlot())
        {
            if (G.currentDrag != null)
                return;
            
            visual.DOKill();
            PlaySlotHoverScale(false);

            _slotMoveTween = visual.DOLocalMove(_visualBaseLocalPos, slotHoverAnimDuration)
                .SetEase(Ease.OutCubic);
            _slotRotateTween = visual.DOLocalRotateQuaternion(_visualBaseLocalRot, slotHoverAnimDuration)
                .SetEase(Ease.OutCubic);

            RestoreOrder();
            return;
        }
        
        PlayHandHoverScale(false);
        RestoreOrder();
        _card?.Hand?.ClearHovered(_card);
    }


    private void PlayScale(Vector3 target)
    {
        _scaleTween?.Kill();
        _scaleTween = visual.DOScale(target, scaleDuration)
            .SetEase(scaleEase)
            .SetUpdate(true);
    }


    private void BringToFront()
    {
        if (_sortingGroup != null)
        {
            _sortingGroup.sortingOrder = _baseSortingOrder + hoverSortingBoost;
            return;
        }

        if (bringToFrontInUIIfNoSortingGroup && transform.parent != null)
        {
            transform.SetAsLastSibling();
        }
    }

    private void RestoreOrder()
    {
        if (_sortingGroup != null)
        {
            _sortingGroup.sortingOrder = _baseSortingOrder;
            return;
        }

        if (bringToFrontInUIIfNoSortingGroup && transform.parent == _baseParent)
        {
            transform.SetSiblingIndex(_baseSiblingIndex);
        }
    }

    private void OnDisable()
    {
        _scaleTween?.Kill();
        visual.localScale = _baseScale;
        RestoreOrder();
    }

    private void PlayHandHoverScale(bool hovered)
    {
        var target = hovered ? _baseScale * handHoverScale : _baseScale;

        _scaleTween?.Kill();
        _scaleTween = visual.DOScale(target, scaleDuration)
            .SetEase(scaleEase)
            .SetUpdate(true);
    }

    private void PlaySlotHoverScale(bool hovered)
    {
        var target = hovered ? _baseScale * slotHoverScale : _baseScale;

        _scaleTween?.Kill();
        _scaleTween = visual.DOScale(target, slotHoverAnimDuration)
            .SetEase(scaleEase)
            .SetUpdate(true);
    }
    
    public void ResetVisual()
    {
        _scaleTween?.Kill();
        _slotMoveTween?.Kill();
        _slotRotateTween?.Kill();

        visual.localScale = _baseScale;
        visual.localPosition = _visualBaseLocalPos;
        visual.localRotation = _visualBaseLocalRot;

        RestoreOrder();
    }
    

}