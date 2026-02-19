using System;
using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "DamageCard", menuName = "Cards/DamageCard", order = 3)]
public class DamageCard : CardModel
{
    public override ClassType ClassType => ClassType.Damage;
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
public class DealDamageToAllInteraction : IOnCardEndTurn
{
    public string desc => "Deals " + DmgAmount + " dmg to all enemies";
    public int DmgAmount;

    public IEnumerator OnEndTurn(CardState state)
    {
        G.enemies.DamageAll(DmgAmount);
        yield return null;
    }
}

[Serializable]
public class DealDamageForEachClassCard : IOnCardEndTurn
{
    public int AddDmg;
    public ClassType classCardsType = ClassType.Damage;

    public string desc =>
        "Deal " + AddDmg + " dmg to target enemy for each` " + classCardsType + " card on field";

    public IEnumerator OnEndTurn(CardState state)
    {
        var count = G.main.field.CountCardsWithClass(classCardsType);
        var target = G.main.Target;
        if (target == null) yield break;
        target.TakeDamage(AddDmg);
    }
}


[Serializable]
public class AddDamageForStatus : IOnCardEndTurn
{
    public int AddDmg;
    public StatusEffectType status;

    public string desc =>
        "Deal " + AddDmg + " dmg to target enemy for each` " + status + " stack on it";

    public IEnumerator OnEndTurn(CardState state)
    {
        var target = G.main.Target;
        if (target == null) yield break;

        var stack = target.StatusTypeStacks(status);

        target.TakeDamage(stack * AddDmg);
    }
}