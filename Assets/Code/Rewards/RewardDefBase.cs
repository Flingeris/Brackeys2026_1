using System.Collections;
using UnityEngine;

public abstract class RewardDefBase : ContentDef, IReward, ITooltipInfo
{
    [field: Header("Reward info")]
    [field: SerializeField]
    public virtual RewardRarity RewardRarity { get; protected set; } = RewardRarity.Common;

    public bool IsExcludedFromRewards;

    [field: Header("Tooltip Info")]
    [field: SerializeField]
    public string ItemName { get; protected set; }

    [TextArea] [field: SerializeField] public virtual string Description { get; protected set; }

    public virtual IEnumerator PickReward()
    {
        yield return OnRewardPicked();
    }

    protected abstract IEnumerator OnRewardPicked();

    public abstract bool CanBePicked();
}