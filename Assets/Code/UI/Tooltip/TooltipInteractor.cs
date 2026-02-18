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

    public void OnPointerEnter(PointerEventData eventData)
    {
        //Debug.Log("Pointer entered: " + gameObject.name);
        if (eventData.pointerCurrentRaycast.gameObject != gameObject)
            return;

        isCurrentlyHovered = true;

        if (InfoGiver != null && G.HUD != null && G.HUD.tooltip != null)
        {
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
