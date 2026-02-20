using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public enum ClassType
{
    None = 0,
    Heal = 1,
    Tank = 2,
    Damage = 3,
}

public enum CardType
{
    None = 0,
    Start = 1,
    Mid = 2,
    End = 3
}


public abstract class CardModel : RewardDefBase, ITooltipInfo
{
    [Header("Card info")] [field: SerializeField] [Header("Card visuals")]
    public Sprite Sprite;


    public override string Description => GetDescription();
    public CardInstance Prefab => "prefabs/Card".Load<CardInstance>();

    [Header("Stats")] public abstract ClassType ClassType { get; }

    public CardType CardType;
    [SerializeReference, SubclassSelector] public List<IOnCardPlayed> OnPlayedCardInteractions;


    [Space(10f)] [SerializeReference, SubclassSelector]
    public List<IOnCardEndTurn> OnTurnEndInteractions;

    public string GetDescription()
    {
        if (OnTurnEndInteractions == null || OnTurnEndInteractions.Count == 0)
            return string.Empty;

        var sb = new StringBuilder();

        for (int i = 0; i < OnTurnEndInteractions.Count; i++)
        {
            var interaction = OnTurnEndInteractions[i];
            if (interaction == null)
                continue;

            if (sb.Length > 0)
                sb.Append('\n');

            sb.Append(interaction.desc);
        }

        return sb.ToString();
    }

    protected override IEnumerator OnRewardPicked()
    {
        G.run.currDeck.Add(new CardState(this));
        // G.Hand.AddCard(new CardState(this));
        yield return null;
    }


    public override bool CanBePicked()
    {
        return true;
    }
}


[Serializable]
public class AddStatusToTargetInteraction : IOnCardEndTurn
{
    [SerializeReference, SubclassSelector] public IStatusEffectInteraction statusToAdd;
    public int stacksToAdd;

    public string desc => "Apply " + stacksToAdd + " Stacks of " + statusToAdd.GetType() + " to target";

    public IEnumerator OnEndTurn(CardState state)
    {
        var target = G.main.Target;
        if (target == null || target.IsDead) yield break;
        target.AddStatus(statusToAdd, stacksToAdd);
    }
}

[Serializable]
public class AddStatusToClassInteractions : IOnCardEndTurn
{
    [SerializeReference, SubclassSelector] public IStatusEffectInteraction statusToAdd;
    public ClassType member;
    public int stacksToAdd;

    public string desc => "Apply " + stacksToAdd + " Stacks of " + statusToAdd.GetType() + " to " + member;

    public IEnumerator OnEndTurn(CardState state)
    {
        var target = G.party.GetMemberByClass(member);
        if (target == null || target.IsDead) yield break;
        target.AddStatus(statusToAdd, stacksToAdd);
    }
}