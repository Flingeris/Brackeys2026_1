using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TankCard", menuName = "Cards/TankCard", order = 2)]
public class TankCard : CardModel
{
    public override ClassType ClassType => ClassType.Tank;
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

[Serializable]
public class ShieldAllForEveryClassCard : IOnCardEndTurn
{
    public int AdditionalShieldAmount;
    public ClassType classCardsType = ClassType.Tank;

    public string desc =>
        "+" + AdditionalShieldAmount + " shield to all allies for each " + classCardsType + " card on field";

    public IEnumerator OnEndTurn(CardState state)
    {
        var counter = G.main.field.CountCardsWithClass(classCardsType);

        G.party.ShieldAll(counter * AdditionalShieldAmount);
        yield break;
    }
}

[Serializable]
public class AddShildToTargetInteraction : IOnCardEndTurn
{
    public int shieldAmount;

    public string desc => "+" + shieldAmount + " shield to target";

    public IEnumerator OnEndTurn(CardState card)
    {
        var target = G.main.Target;
        if (target == null) yield break;
        target.AddShield(shieldAmount);
    }
}
