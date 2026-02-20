using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class RewardSlot : MonoBehaviour, IPointerClickHandler, ITooltipInfoGiver
{
    public int Index = 0;
    public event UnityAction OnContainerChanged;

    public event UnityAction<RewardDefBase> OnRewardSlotClicked;

    public RewardDefBase Reward { get; private set; }

    [SerializeField] private SpriteRenderer rewardIcon;
    [SerializeField] private Collider2D clickCollider;

    private CardInstance cardInstance;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (Reward == null) return;
        OnRewardSlotClicked?.Invoke(Reward);   
        G.Hand.Claim(cardInstance);
        
    }

    public void SetRewardInSlot(RewardDefBase newReward)
    {
        
        cardInstance = null;
        if(newReward == null) return;
        this.Reward = newReward;
        bool hasReward = Reward != null;
        

        if (clickCollider) clickCollider.enabled = hasReward;

        cardInstance = SpawnReward(newReward);
        var go = cardInstance.gameObject;
        go.transform.parent = transform;
        go.transform.DOKill();
        go.transform.DOLocalMove(Vector3.zero, 0.5f);
        go.transform.localScale = Vector3.one;


        OnContainerChanged?.Invoke();
    }

    private CardInstance SpawnReward(RewardDefBase reward)
    {
        if (reward is CardModel card)
        {
            return G.Hand.CreateCard(card.Id);
        }

        return null;
    }

    public ITooltipInfo GetTooltipInfo()
    {
        if (Reward != null && Reward is ITooltipInfo rewInf) return rewInf;
        else return null;
    }
}