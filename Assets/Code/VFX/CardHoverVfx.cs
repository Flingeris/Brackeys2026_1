using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using UnityEngine.Rendering;

public class CardHoverVFX : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Scale")]
    [SerializeField] private float hoverScale = 1.12f;
    [SerializeField] private float scaleDuration = 0.12f;
    [SerializeField] private Ease scaleEase = Ease.OutBack;

    [Header("Sorting / Front")]
    [SerializeField] private int hoverSortingBoost = 200; // насколько "поднять" относительно базового
    [SerializeField] private bool bringToFrontInUIIfNoSortingGroup = true;

    private Vector3 _baseScale;
    private Tween _scaleTween;

    private SortingGroup _sortingGroup;
    private int _baseSortingOrder;

    private Transform _baseParent;
    private int _baseSiblingIndex;
    
    private CardInstance _card;
    private Draggable _draggable;
    private bool _isDragging;

    private void Awake()
    {
        _card = GetComponent<CardInstance>() ?? GetComponentInParent<CardInstance>();
        _draggable = GetComponentInParent<Draggable>();

        if (_draggable != null)
        {
            _draggable.OnDragBegin += HandleDragBegin;
            _draggable.OnDragEnd += HandleDragEnd;
        }
        
        _baseScale = transform.localScale;

        _sortingGroup = GetComponent<SortingGroup>();
        if (_sortingGroup != null)
            _baseSortingOrder = _sortingGroup.sortingOrder;

        _baseParent = transform.parent;
        _baseSiblingIndex = transform.GetSiblingIndex();
        
        _draggable = GetComponentInParent<Draggable>();
        if (_draggable != null)
        {
            _draggable.OnDragBegin += HandleDragBegin;
            _draggable.OnDragEnd += HandleDragEnd;
        }
    }
    
    private void HandleDragBegin()
    {
        _isDragging = true;
        _card?.Hand?.ClearHovered(_card);  // чтобы при драге рука не пыталась “раскладывать” эту карту
    }


    private void HandleDragEnd()
    {
        _isDragging = false;
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_isDragging) return;

        PlayScale(_baseScale * hoverScale);
        BringToFront();

        _card?.Hand?.SetHovered(_card);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_isDragging) return;

        PlayScale(_baseScale);
        RestoreOrder();

        // ВАЖНО: снова очищаем hovered
        _card?.Hand?.ClearHovered(_card);
    }



    private void PlayScale(Vector3 target)
    {
        _scaleTween?.Kill();
        _scaleTween = transform.DOScale(target, scaleDuration)
            .SetEase(scaleEase)
            .SetUpdate(true); // чтобы работало даже если ты потом будешь паузить Time.timeScale
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
        transform.localScale = _baseScale;
        RestoreOrder();
    }
}
