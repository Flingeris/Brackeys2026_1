using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipInteractor : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private ITooltipInfoGiver InfoGiver;
    private bool isCurrentlyHovered = false;

    private void Awake()
    {
        InfoGiver = GetComponent<ITooltipInfoGiver>();
        if (InfoGiver == null)
        {
            Debug.LogError("Can't find tooltip giver attached to object " + gameObject.name);
        }
    }

    void Update()
    {
        // Позиция мыши на экране (пиксели)
        Vector3 mouseScreenPos = Input.mousePosition;

        // Переводим в мировые координаты
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
        // Для 2D важно обнулить Z до плоскости, где лежат объекты
        mouseWorldPos.z = 0f;

        // Луч без направления (Vector2.zero) — просто точечный хит-тест в этой позиции
        RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero);

        if (hit.collider != null)
        {
            // Логируем объект под мышкой
            Debug.Log("Hover: " + hit.collider.gameObject.name);
        }
        else
        {
            // Если никто не пойман — по желанию:
            // Debug.Log("Hover: ничего нет");
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("Pointer entered: " + gameObject.transform.parent.name);
        Debug.Log("Pointer current Raycast: " + eventData.pointerCurrentRaycast.gameObject.transform.parent.name);
        if (eventData.pointerCurrentRaycast.gameObject != gameObject)
            return;

        isCurrentlyHovered = true;

        if (InfoGiver != null && G.HUD != null && G.HUD.tooltip != null)
        {
            Debug.Log("Showing tooltip");
            G.HUD.tooltip.Show(InfoGiver.GetTooltipInfo(), gameObject);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isCurrentlyHovered)
            return;

        isCurrentlyHovered = false;

        if (G.HUD != null && G.HUD.tooltip != null)
        {
            G.HUD.tooltip.Hide();
        }
    }
}

public class RewardInteractor: MonoBehaviour, IPointerClickHandler
{
    // [SerializeField] private IReward
    
    public void OnPointerClick(PointerEventData eventData)
    {
        throw new System.NotImplementedException();
    }
}



















