using UnityEngine;

using UnityEngine;

public class TooltipStatusEffect : ITooltipInfo
{
    public string ItemName { get; }
    public string Description { get; }
    public Sprite Icon { get; }

    public TooltipStatusEffect(string itemName, string description, Sprite icon)
    {
        ItemName = itemName;
        Description = description;
        Icon = icon;
    }
}