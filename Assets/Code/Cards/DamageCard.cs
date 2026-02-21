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
    public string desc => $"Deal {TextStuff.ColoredValue(DmgAmount, TextStuff.Damage)} to random enemy";
    
    public int DmgAmount;

    public IEnumerator OnEndTurn(CardState state)
    {
        var enemy = G.enemies.GetRandomMember();
        if (enemy == null) yield break;
        yield return enemy.TakeDamage(DmgAmount);
        yield return null;
    }
}

[Serializable]
public class DealDamageToTarget : IOnCardEndTurn
{
    public string desc => $"Deal {TextStuff.ColoredValue(DmgAmount, TextStuff.Damage)} to chosen enemy";
    
    public int DmgAmount;

    public IEnumerator OnEndTurn(CardState state)
    {
        var target = G.main.Target;
        if (target == null) yield break;

      yield return  target.TakeDamage(DmgAmount);
        yield return null;
    }
}

[Serializable]
public class DealDamageToAllInteraction : IOnCardEndTurn
{
    public string desc => $"Deal {TextStuff.ColoredValue(DmgAmount, TextStuff.Damage)} to all enemies";
    public int DmgAmount;

    public IEnumerator OnEndTurn(CardState state)
    {
       yield return G.enemies.DamageAll(DmgAmount);
        yield return null;
    }
}

[Serializable]
public class DealDamageForEachClassCard : IOnCardEndTurn
{
    public int AddDmg;
    public ClassType classCardsType = ClassType.Damage;

    public string desc => $"Deal {TextStuff.ColoredValue(AddDmg, TextStuff.Damage)} for each {TextStuff.GetClassTypeString(classCardsType)} card on field";

    public IEnumerator OnEndTurn(CardState state)
    {
        var count = G.main.field.CountCardsWithClass(classCardsType);
        var target = G.main.Target;
        if (target == null) yield break;
      yield return  target.TakeDamage(AddDmg);
    }
}


[Serializable]
public class AddDamageForStatus : IOnCardEndTurn, INoAnimationAction
{
    public int AddDmg;
    public StatusEffectType status;
    
    public string desc => $"Deals additional {TextStuff.ColoredValue(AddDmg, TextStuff.Damage)} for each {TextStuff.GetStatus(status)} stack";

    public IEnumerator OnEndTurn(CardState state)
    {
        var target = G.main.Target;
        if (target == null) yield break;

        var stack = target.StatusTypeStacks(status);

        yield return target.TakeDamage(stack * AddDmg);
    }
}