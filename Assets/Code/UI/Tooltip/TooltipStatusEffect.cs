using UnityEngine;

public class TooltipStatusEffect : ITooltipInfo
{
    public string ItemName { get; }
    public string Description { get; }

    public TooltipStatusEffect(string itemName, string description)
    {
        ItemName = itemName;
        Description = description;
    }
}