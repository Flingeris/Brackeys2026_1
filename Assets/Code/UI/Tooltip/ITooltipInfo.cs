public interface ITooltipInfo
{
    string ItemName { get; }
    string Description { get; }
}

public interface ITooltipInfoGiver
{
    ITooltipInfo GetTooltipInfo();
}