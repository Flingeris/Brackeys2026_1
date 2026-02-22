using TMPro;
using UnityEngine;

public class StatusEffectView : MonoBehaviour, ITooltipInfoGiver
{
    [SerializeField] private SpriteRenderer iconRenderer;
    [SerializeField] private TMP_Text stacksText;

    private IStatusEffectInteraction effect;


    private void OnValidate()
    {
        if (iconRenderer == null)
            iconRenderer = GetComponentInChildren<SpriteRenderer>();

        if (stacksText == null)
            stacksText = GetComponentInChildren<TMP_Text>();
    }

    public void Setup(IStatusEffectInteraction effect)
    {
        this.effect = effect;
        UpdateVisuals();
    }


    private void UpdateVisuals()
    {
        if (effect == null)
        {
            if (iconRenderer != null) iconRenderer.enabled = false;
            if (stacksText != null) stacksText.SetText(string.Empty);
            return;
        }

        var icon = effect.GetSprite();
        var stacks = effect.Stacks;

        if (iconRenderer == null || stacksText == null)
            return;

        iconRenderer.sprite = icon;
        iconRenderer.enabled = icon != null;

        if (stacks > 1)
            stacksText.SetText(stacks.ToString());
        else
            stacksText.SetText(string.Empty);
    }

    public ITooltipInfo GetTooltipInfo()
    {
        return effect;
    }
}