using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

[Serializable]
public class ActionDef
{
    [SerializeReference, SubclassSelector] public List<IOnEnemyTurnEnd> OnEndTurnInteractions;
}


public interface IEnemyInteraction
{
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
public class DealDamageInteraction : IOnEnemyTurnEnd
{
    public int[] possibleTargets;
    public int DmgAmount;
    public string desc => "Will deal " + DmgAmount + " damage";

    public IEnumerator OnEndTurn(EnemyInstance e)
    {
        var aliveTargets = possibleTargets.Where(i => G.party.CheckIsAlive(i)).ToArray();

        if (aliveTargets.Length == 0)
            yield break;

        var targetIndx = aliveTargets.GetRandomElement();
        G.party.DealDamage(targetIndx, DmgAmount);
    }
}

[Serializable]
public class DealDamageToRandomTarget : IOnEnemyTurnEnd
{
    public int DmgAmount;
    public string desc => "Will deal " + DmgAmount + " to random target";

    public IEnumerator OnEndTurn(EnemyInstance e)
    {
        var target = G.party.GetRandomMember();

        if (target == null || target.IsDead)
            yield break;


        target.TakeDamage(DmgAmount);
    }
}

[Serializable]
public class HealLowestTarget : IOnEnemyTurnEnd
{
    public int HealAmount;
    public string desc => "Will heal ally";

    public IEnumerator OnEndTurn(EnemyInstance e)
    {
        var target = G.enemies.GetRandomMember();

        if (target == null || target.IsDead)
            yield break;


        target.Heal(HealAmount);
    }
}

[Serializable]
public class HealAllAllies : IOnEnemyTurnEnd
{
    public int HealAmount;
    public string desc => "Will heal ally";

    public IEnumerator OnEndTurn(EnemyInstance e)
    {
        G.enemies.HealAll(HealAmount);
        yield return null;
    }
}