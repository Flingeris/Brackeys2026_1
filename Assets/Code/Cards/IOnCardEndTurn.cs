using System;
using System.Collections;

public interface IOnCardEndTurn : ICardInteraction
{
    public string desc { get; }
    public IEnumerator OnEndTurn(CardState card);
}


[Serializable]
public class ChooseTargetOnTurnEndInteraction : ChooseTargetInteractionBase, IOnCardEndTurn
{
    public string desc => string.Empty;

    public IEnumerator OnEndTurn(CardState state)
    {
        yield return OnTargetChoose();
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
public class RandomTargetDamageInteraction : IOnCardEndTurn
{
    public string desc => "Deals " + DmgAmount + " dmg to random target";
    public int DmgAmount;

    public IEnumerator OnEndTurn(CardState state)
    {
        var enemy = G.enemies.GetRandomMember();
        if (enemy == null) yield break;
        enemy.TakeDamage(DmgAmount);
        yield return null;
    }
}

[Serializable]
public class DealDamageToTarget : IOnCardEndTurn
{
    public string desc => "Deals " + DmgAmount + " dmg to chosen target";
    public int DmgAmount;

    public IEnumerator OnEndTurn(CardState state)
    {
        var target = G.main.Target;
        if (target == null) yield break;

        target.TakeDamage(DmgAmount);
        yield return null;
    }
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
public class AdditionalHealForEachHealCardOnFieldInteraction : IOnCardEndTurn
{
    public int AdditionalHealAmount;

    public string desc =>
        "+" + AdditionalHealAmount + " hp to all allies for each healer card played this turn";

    public IEnumerator OnEndTurn(CardState state)
    {
        var addHeal = 0;
        foreach (var card in G.main.field.PlayedCards)
        {
            if (card == null) continue;
            if (card.state.model.ClassType is ClassType.Heal)
            {
                addHeal++;
            }
        }

        G.party.HealAll(addHeal * AdditionalHealAmount);
        yield break;
    }
}




[Serializable]
public class AddShieldToClassInteraction : IOnCardEndTurn
{
    public string desc => "+" + ShieldAmount + " shield to " + ClassType;
    public int ShieldAmount;
    public ClassType ClassType = ClassType.Tank;

    public IEnumerator OnEndTurn(CardState state)
    {
        var memberByClass = G.party.GetMemberByClass(ClassType);
        if (memberByClass == null) yield break;
        memberByClass.AddShield(ShieldAmount);
    }
}

[Serializable]
public class AddTauntChargesInteraction : IOnCardEndTurn
{
    public string desc => "+" + ShieldAmount + " shield to " + ClassType;
    public int ShieldAmount;
    public ClassType ClassType = ClassType.Tank;

    public IEnumerator OnEndTurn(CardState state)
    {
        var memberByClass = G.party.GetMemberByClass(ClassType);
        if (memberByClass == null) yield break;
    }
}


[Serializable]
public class ShieldAllInteraction : IOnCardEndTurn
{
    public string desc => "+" + ShieldAmount + " shield to all allies";
    public int ShieldAmount;

    public IEnumerator OnEndTurn(CardState state)
    {
        G.party.ShieldAll(ShieldAmount);
        yield break;
    }
}