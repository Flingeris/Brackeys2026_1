using TMPro;
using UnityEngine;

public class StatusIconView : MonoBehaviour
{
    [SerializeField] private SpriteRenderer iconRenderer;
    [SerializeField] private TMP_Text stacksText;

    private void OnValidate()
    {
        if (iconRenderer == null)
            iconRenderer = GetComponentInChildren<SpriteRenderer>();

        if (stacksText == null)
            stacksText = GetComponentInChildren<TMP_Text>();
    }

    public void Setup(Sprite icon, int stacks)
    {
        if (iconRenderer == null || stacksText == null)
            return;

        iconRenderer.sprite = icon;
        iconRenderer.enabled = icon != null;
        
        if (stacks > 1)
            stacksText.SetText(stacks.ToString());
        else
            stacksText.SetText(string.Empty);
    }
}