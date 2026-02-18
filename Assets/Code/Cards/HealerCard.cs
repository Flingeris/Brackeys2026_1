using System;
using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "HealerCard", menuName = "Cards/HealerCard", order = 1)]
public class HealerCard : CardModel
{
    public override ClassType ClassType => ClassType.Heal;
}


[Serializable]
public class HealWeakestInteraction : IOnCardEndTurn
{
    public int HealAmount;
    public float AdditionalHealCondition;
    public int AdditionalHealAmount;

    public string desc => "+" + HealAmount + " hp to lowest ally. If ally hp is <=" + AdditionalHealCondition +
                          " +" + AdditionalHealAmount + "hp instead";

    public IEnumerator OnEndTurn(CardState state)
    {
        var lowest = G.party.GetLowestMember();
        if (lowest == null) yield break;
        if (lowest.CurrHP <= AdditionalHealCondition)
        {
            lowest.Heal(AdditionalHealAmount);
        }
        else
        {
            lowest.Heal(HealAmount);
        }

        yield return null;
    }
}

[Serializable]
public class HealAllForEachClassCardInteraction : IOnCardEndTurn
{
    public int AdditionalHealAmount;
    public ClassType classCardsType = ClassType.Heal;

    public string desc =>
        "+" + AdditionalHealAmount + " hp to all allies for each + " + classCardsType + " card on field";

    public IEnumerator OnEndTurn(CardState state)
    {
        var addHeal = 0;
        foreach (var card in G.main.field.PlayedCards)
        {
            if (card == null) continue;
            if (card.state.model.ClassType == classCardsType)
            {
                addHeal++;
            }
        }

        G.party.HealAll(addHeal * AdditionalHealAmount);
        yield break;
    }
}


[Serializable]
public class TargetHealInteraction : IOnCardEndTurn
{
    public int HealAmount;

    public string desc => "+" + HealAmount + " hp to target";

    public IEnumerator OnEndTurn(CardState card)
    {
        var target = G.main.Target;
        if (target == null) yield break;
        target.Heal(HealAmount);
    }
}


[Serializable]
public class HealAllInteraction : IOnCardEndTurn
{
    public int HealAmount;
    public string desc => "+" + HealAmount + " hp to all allies";


    public IEnumerator OnEndTurn(CardState state)
    {
        G.party.HealAll(HealAmount);
        yield break;
    }
}

[Serializable]
public class TargetRandomHealInteraction : IOnCardEndTurn
{
    public int MinHealAmount;
    public int MaxHealAmount;
    public string desc => "+ " + MinHealAmount + "-" + MaxHealAmount + " hp to target ally";


    public IEnumerator OnEndTurn(CardState state)
    {
        var target = G.main.Target;
        if (target == null) yield break;
        var heal = UnityEngine.Random.Range(MinHealAmount, MaxHealAmount);
        target.Heal(heal);
    }
}