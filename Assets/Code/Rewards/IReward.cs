using System.Collections;
using UnityEngine;

public interface IReward : ITooltipInfo

{
    RewardRarity RewardRarity { get; }

    IEnumerator PickReward();

    bool CanBePicked();
}


public enum RewardRarity
{
    Common,
    Uncommon,
    Rare,
}