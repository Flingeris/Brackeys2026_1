using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;


public interface IEnemyInteraction
{
    public InteractionType type { get; }
}

public interface IAmountInteraction
{
    string GetAmountAsString();
}


[CreateAssetMenu(fileName = "EnemyModel", menuName = "Enemies/EnemyModel")]
public class EnemyModel : ContentDef, ITooltipInfo
{
    [Header("Enemy info")]
    [field: SerializeField]
    public string ItemName { get; private set; }

    public SoundId damagedSound;

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
public class DealDamageToPosInteraction : IOnEnemyTurnEnd, IAmountInteraction
{
    public int[] possibleTargets;
    public int damageAmount;
    public string desc => $"{TextStuff.TurnEnd} deal {TextStuff.ColoredValue(damageAmount, TextStuff.Damage)}";

    public IEnumerator OnEndTurn(EnemyInstance e)
    {
        var aliveTargets = possibleTargets.Where(i => G.party.CheckIsAlive(i)).ToArray();

        if (aliveTargets.Length == 0)
            yield break;

        var targetIndx = aliveTargets.GetRandomElement();
        var finalDmg = e.GetFinalDamage(damageAmount);
        yield return G.party.DealDamage(targetIndx, finalDmg);
    }

    public InteractionType type => InteractionType.Attack;

    public string GetAmountAsString()
    {
        return damageAmount.ToString();
    }
}

[Serializable]
public class DealDamageToRandomTarget : IOnEnemyTurnEnd, IAmountInteraction
{
    public int damageAmount;

    public string desc =>
        $"{TextStuff.TurnEnd} deal {TextStuff.ColoredValue(damageAmount, TextStuff.Damage)} to a random target";

    public IEnumerator OnEndTurn(EnemyInstance e)
    {
        var target = e.currentTarget;

        if (target == null || target.IsDead)
            yield break;

        var finalDmg = e.GetFinalDamage(damageAmount);
        yield return target.TakeDamage(finalDmg);
    }

    public InteractionType type => InteractionType.Attack;

    public string GetAmountAsString()
    {
        return damageAmount.ToString();
    }
}

[Serializable]
public class DamageAllRange : IOnEnemyTurnEnd, IAmountInteraction
{
    public int minDamage;
    public int maxDamage;

    public string desc =>
        $"{TextStuff.TurnEnd} deal {TextStuff.ColoredRange(minDamage, maxDamage, TextStuff.Damage)} to party members";


    public IEnumerator OnEndTurn(EnemyInstance e)
    {
        var modMin = e.GetFinalDamage(minDamage);
        var modMax = e.GetFinalDamage(maxDamage);
        yield return G.party.DamageAllRange(modMin, modMax);
    }

    public InteractionType type => InteractionType.Attack;

    public string GetAmountAsString()
    {
        return $"{minDamage}-{maxDamage}";
    }
}


[Serializable]
public class HealLowestTarget : IOnEnemyTurnEnd, IAmountInteraction
{
    public int HealAmount;

    public string desc =>
        $"{TextStuff.TurnEnd} Heal lowest ally for {TextStuff.ColoredValue(HealAmount, TextStuff.Hp)}";

    public IEnumerator OnEndTurn(EnemyInstance e)
    {
        var target = G.enemies.GetLowestMember();

        if (target == null || target.IsDead)
            yield break;


        target.Heal(HealAmount);
    }

    public InteractionType type => InteractionType.Heal;

    public string GetAmountAsString()
    {
        return HealAmount.ToString();
    }
}

[Serializable]
public class HealAllAllies : IOnEnemyTurnEnd, IAmountInteraction
{
    public int HealAmount;
    public string desc => $"{TextStuff.TurnEnd} Heal all allies for {TextStuff.ColoredValue(HealAmount, TextStuff.Hp)}";

    public IEnumerator OnEndTurn(EnemyInstance e)
    {
        yield return G.enemies.HealAll(HealAmount);
        yield return null;
    }

    public InteractionType type => InteractionType.Heal;

    public string GetAmountAsString()
    {
        return HealAmount.ToString();
    }
}

[Serializable]
public class HealAtPosition : IOnEnemyTurnEnd, IAmountInteraction
{
    public int[] possibleTargets;
    public int HealAmount;
    public string desc => $"{TextStuff.TurnEnd} Heal ally for {TextStuff.ColoredValue(HealAmount, TextStuff.Hp)}";

    public IEnumerator OnEndTurn(EnemyInstance e)
    {
        var aliveTargets = possibleTargets.Where(i => G.party.CheckIsAlive(i)).ToArray();

        if (aliveTargets.Length == 0)
            yield break;

        var targetIndx = aliveTargets.GetRandomElement();
        G.enemies.Heal(targetIndx, HealAmount);
    }

    public InteractionType type => InteractionType.Heal;

    public string GetAmountAsString()
    {
        return HealAmount.ToString();
    }
}


[Serializable]
public class ChooseRandomTarget : IOnEnemyTurnEnd, INoAnimationAction
{
    public InteractionType type => InteractionType.None;
    public string desc { get; }

    public IEnumerator OnEndTurn(EnemyInstance e)
    {
        var target = G.party.GetRandomMember();
        if (target == null || target.IsDead) yield break;
        e.currentTarget = target;
    }
}

[Serializable]
public class ShieldAlliesOnPos : IOnEnemyTurnEnd, IAmountInteraction
{
    public int[] possibleTargets;
    public int shieldAmount;

    public string desc =>
        $"{TextStuff.TurnEnd} Grant {TextStuff.ColoredValue(shieldAmount, TextStuff.Shield)} to allies";

    public IEnumerator OnEndTurn(EnemyInstance e)
    {
        var aliveTargets = possibleTargets.Where(i => G.enemies.CheckIsAlive(i)).ToArray();

        if (aliveTargets.Length == 0)
            yield break;

        foreach (var target in aliveTargets)
        {
            G.enemies.AddShield(target, shieldAmount);
        }
    }

    public InteractionType type => InteractionType.Shield;

    public string GetAmountAsString()
    {
        return shieldAmount.ToString();
    }
}

[Serializable]
public class ShieldOnSelf : IOnEnemyTurnEnd, IAmountInteraction
{
    public int shieldAmount;

    public string desc =>
        $"{TextStuff.TurnEnd} Grant {TextStuff.ColoredValue(shieldAmount, TextStuff.Shield)} to yourself";

    public IEnumerator OnEndTurn(EnemyInstance e)
    {
        if (e == null || e.IsDead) yield break;
        e.AddShield(shieldAmount);
    }

    public InteractionType type => InteractionType.Shield;

    public string GetAmountAsString()
    {
        return shieldAmount.ToString();
    }
}

[Serializable]
public class ShieldAllEnemies : IOnEnemyTurnEnd, IAmountInteraction
{
    public int shieldAmount;

    public string desc =>
        $"{TextStuff.TurnEnd} Grant {TextStuff.ColoredValue(shieldAmount, TextStuff.Shield)} to all enemies";

    public IEnumerator OnEndTurn(EnemyInstance e)
    {
        G.enemies.ShieldAll(shieldAmount);
        yield break;
    }

    public InteractionType type => InteractionType.Shield;

    public string GetAmountAsString()
    {
        return shieldAmount.ToString();
    }
}


[Serializable]
public class ApplyStatusOnCurrTargetCharacter : IOnEnemyTurnEnd, IAmountInteraction, INoAnimationAction
{
    [SerializeReference, SubclassSelector] public IStatusEffectInteraction statusToAdd;
    public int stacksToAdd;

    public string desc => $"Apply {stacksToAdd} stack(s) of {TextStuff.GetStatus(statusToAdd.Type)}";


    public IEnumerator OnEndTurn(EnemyInstance e)
    {
        var target = e.currentTarget;
        if (target == null || target.IsDead) yield break;
        target.AddStatus(statusToAdd, stacksToAdd);
    }


    public InteractionType type => InteractionType.Debuff;

    public string GetAmountAsString()
    {
        return stacksToAdd.ToString();
    }
}

[Serializable]
public class ApplyStatusOnSelf : IOnEnemyTurnEnd, IAmountInteraction, INoAnimationAction
{
    [SerializeReference, SubclassSelector] public IStatusEffectInteraction statusToAdd;
    public int stacksToAdd;

    public string desc =>
        $"{TextStuff.TurnEnd} Apply {stacksToAdd} stack(s) of {TextStuff.GetStatus(statusToAdd.Type)} on yourself";


    public IEnumerator OnEndTurn(EnemyInstance e)
    {
        if (e == null || e.IsDead) yield break;
        e.AddStatus(statusToAdd, stacksToAdd);
    }


    public InteractionType type => InteractionType.Buff;

    public string GetAmountAsString()
    {
        return stacksToAdd.ToString();
    }
}

[Serializable]
public class ApplyStatusOnAllCharacters : IOnEnemyTurnEnd, IAmountInteraction, INoAnimationAction
{
    [SerializeReference, SubclassSelector] public IStatusEffectInteraction statusToAdd;
    public int stacksToAdd;

    public string desc =>
        $"Apply {stacksToAdd} stack(s) of {TextStuff.GetStatus(statusToAdd.Type)} on all party members";


    public IEnumerator OnEndTurn(EnemyInstance e)
    {
        var targets = G.party.GetAliveMembers();
        foreach (var target in targets)
        {
            if (target == null || target.IsDead) continue;
            target.AddStatus(statusToAdd, stacksToAdd);
            // yield return new WaitForSeconds(0.2f);
        }

        yield break;
    }


    public InteractionType type => InteractionType.Debuff;

    public string GetAmountAsString()
    {
        return stacksToAdd.ToString();
    }
}


[Serializable]
public class ApplyStatusOnPositions : IOnEnemyTurnEnd, IAmountInteraction, INoAnimationAction
{
    public int[] possibleTargets;
    [SerializeReference, SubclassSelector] public IStatusEffectInteraction statusToAdd;
    public int stacksToAdd;

    public string desc =>
        $"Apply {stacksToAdd} stack(s) of {TextStuff.GetStatus(statusToAdd.Type)} on enemy";


    public IEnumerator OnEndTurn(EnemyInstance e)
    {
        var aliveTargets = possibleTargets.Where(i => G.enemies.CheckIsAlive(i)).ToArray();

        if (aliveTargets.Length == 0)
            yield break;

        var finalTargetIndex = aliveTargets.GetRandomElement();
        var target = G.enemies.GetMember(finalTargetIndex);

        target.AddStatus(statusToAdd, stacksToAdd);
    }


    public InteractionType type => InteractionType.Buff;

    public string GetAmountAsString()
    {
        return stacksToAdd.ToString();
    }
}