using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;


public interface IEnemyInteraction
{
    public InteractionType type { get; }
}

public interface IAmountInteraction
{
    int GetAmount();
}


[CreateAssetMenu(fileName = "EnemyModel", menuName = "Enemies/EnemyModel")]
public class EnemyModel : ContentDef, ITooltipInfo
{
    [Header("Enemy info")]
    [field: SerializeField]
    public string ItemName { get; private set; }

    [field: SerializeField] public string Description { get; private set; }
    [Header("Stats")] public int StartingHealth;
    public int preferPos;
    public int Speed;

    [Header("Enemy Visuals")] public Sprite Sprite;

    public EnemyInstance Prefab => "prefabs/Enemy".Load<EnemyInstance>();


    [SerializeField] public List<ActionDef> EndTurnActions = new List<ActionDef>();
}

public interface IOnEnemyTurnEnd : IEnemyInteraction
{
    public string desc { get; }
    public IEnumerator OnEndTurn(EnemyInstance e);
}


[Serializable]
public class DealDamageInteraction : IOnEnemyTurnEnd, IAmountInteraction
{
    public int[] possibleTargets;
    public int damageAmount;
    public string desc => "Will deal " + damageAmount + " damage";

    public IEnumerator OnEndTurn(EnemyInstance e)
    {
        var aliveTargets = possibleTargets.Where(i => G.party.CheckIsAlive(i)).ToArray();

        if (aliveTargets.Length == 0)
            yield break;

        var targetIndx = aliveTargets.GetRandomElement();
        G.party.DealDamage(targetIndx, damageAmount);
    }

    public InteractionType type => InteractionType.Attack;

    public int GetAmount()
    {
        return damageAmount;
    }
}

[Serializable]
public class DealDamageToRandomTarget : IOnEnemyTurnEnd, IAmountInteraction
{
    public int damageAmount;
    public string desc => "Will deal " + damageAmount + " dmg to random target";

    public IEnumerator OnEndTurn(EnemyInstance e)
    {
        var target = G.party.GetRandomMember();

        if (target == null || target.IsDead)
            yield break;


        target.TakeDamage(damageAmount);
    }

    public InteractionType type => InteractionType.Attack;

    public int GetAmount()
    {
        return damageAmount;
    }
}

[Serializable]
public class HealLowestTarget : IOnEnemyTurnEnd, IAmountInteraction
{
    public int HealAmount;
    public string desc => "Will heal allies ";

    public IEnumerator OnEndTurn(EnemyInstance e)
    {
        var target = G.enemies.GetLowestMember();

        if (target == null || target.IsDead)
            yield break;


        target.Heal(HealAmount);
    }

    public InteractionType type => InteractionType.Heal;

    public int GetAmount()
    {
        return HealAmount;
    }
}

[Serializable]
public class HealAllAllies : IOnEnemyTurnEnd, IAmountInteraction
{
    public int HealAmount;
    public string desc => "Will heal allies";

    public IEnumerator OnEndTurn(EnemyInstance e)
    {
        G.enemies.HealAll(HealAmount);
        yield return null;
    }

    public InteractionType type => InteractionType.Heal;

    public int GetAmount()
    {
        return HealAmount;
    }
}

