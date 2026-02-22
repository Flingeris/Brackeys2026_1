using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

public class RewardManager : MonoBehaviour
{
    public event UnityAction OnPicked;
    public static List<RewardDefBase> pickedRewards = new();

    [Header("Reward View")] [SerializeField]
    private RewardSlot[] rewardSlots;

    [SerializeField] private SpriteRenderer Blackout;
    private RewardDefBase pickedReward;

    private Coroutine rewardCoroutine;
    public bool IsActive { get; private set; }

    private void Awake()
    {
        G.Reward = this;

        InitRewardSlots();
        Blackout.gameObject.SetActive(false);
    }


    private void InitRewardSlots()
    {
        for (var i = 0; i < rewardSlots.Length; i++)
        {
            rewardSlots[i].OnRewardSlotClicked += HandleRewardClick;
            rewardSlots[i].Index = i;
            rewardSlots[i].SetRewardInSlot(null);
        }
    }

    public IEnumerator StartRewarding<T>() where T : RewardDefBase
    {
        IsActive = true;
        yield return RewardingSequence<T>();
    }

    private IEnumerator RewardingSequence<T>() where T : RewardDefBase
    {
        Blackout.gameObject.SetActive(true);
        SetupRewards<T>();

        while (IsActive) yield return null;

        yield return HideRewards();
        yield return new WaitForSeconds(0.25f);
        if (pickedReward != null) yield return pickedReward.PickReward();

        pickedReward = null;
        Blackout.gameObject.SetActive(false);
    }

    public void EndRewarding()
    {
        OnPicked?.Invoke();
        IsActive = false;
    }

    private void SetupRewards<T>() where T : RewardDefBase
    {
        var pool = BuildPool<T>();

        for (var i = 0; i < rewardSlots.Length; i++)
        {
            var reward = pool[i % pool.Count];
            rewardSlots[i].SetRewardInSlot(reward);
            
            if (reward != null)
            {
                AnalyticsSystem.OnRewardOffered(
                    reward.Id,
                    reward.GetType().Name,
                    i,
                    G.run != null ? G.run.mapNodeIndex : -1
                );
            }
        }
    }

    private List<T> BuildPool<T>() where T : RewardDefBase
    {
        var allRewards = CMS.GetAll<T>().Where(r =>
            !r.IsExcludedFromRewards
            && !pickedRewards.Contains(r)).ToList();

        if (allRewards.Count == 0)
        {
            foreach (var slot in rewardSlots) slot.SetRewardInSlot(null);
            Debug.LogWarning("No rewards to setup.");
            return null;
        }

        allRewards.Shuffle();
        return allRewards;
    }


    public void HandleRewardClick(RewardDefBase reward)
    {
        if (rewardCoroutine != null) return;
        rewardCoroutine = StartCoroutine(OnRewardClick(reward));
    }

    private IEnumerator OnRewardClick(RewardDefBase reward)
    {
        if (reward.CanBePicked())
        {
            pickedReward = reward;
            pickedRewards.Add(reward);
            EndRewarding();
        }
        else
        {
            G.HUD.Say("You cannot pick this reward right now.");
        }

        rewardCoroutine = null;
        yield break;
    }

    public IEnumerator HideRewards()
    {
        foreach (var slot in rewardSlots)
        {
            yield return slot.transform.DOPunchScale(new Vector3(0.4f, 0.4f), 0.2f, 3).WaitForCompletion();
            slot.SetRewardInSlot(null);
        }
    }
}