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

    public string desc =>
        $"Heal lowest-HP ally for {TextStuff.ColoredValue(HealAmount, TextStuff.Hp)}. " +
        $"If HP is lower than {AdditionalHealCondition} Heals {TextStuff.ColoredValue(AdditionalHealAmount, TextStuff.Hp)} instead";
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
        $"Heal all allies for {TextStuff.ColoredValue(AdditionalHealAmount, TextStuff.Hp)} for each {TextStuff.GetClassTypeString(classCardsType)} card on field";

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

        yield return  G.party.HealAll(addHeal * AdditionalHealAmount);
    }
}


[Serializable]
public class TargetHealInteraction : IOnCardEndTurn
{
    public int HealAmount;

    public string desc => $"Heal chosen ally for {TextStuff.ColoredValue(HealAmount, TextStuff.Hp)}";

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
    public string desc => $"Heal all allies for {TextStuff.ColoredValue(HealAmount, TextStuff.Hp)}";


    public IEnumerator OnEndTurn(CardState state)
    {
       yield return G.party.HealAll(HealAmount);
        yield break;
    }
}

[Serializable]
public class TargetRandomHealInteraction : IOnCardEndTurn
{
    public int MinHealAmount;
    public int MaxHealAmount;
    public string desc => $"Heal chosen ally for {TextStuff.ColoredRange(MinHealAmount, MaxHealAmount, TextStuff.Hp)}";


    public IEnumerator OnEndTurn(CardState state)
    {
        var target = G.main.Target;
        if (target == null) yield break;
        var heal = UnityEngine.Random.Range(MinHealAmount, MaxHealAmount);
        target.Heal(heal);
    }
}