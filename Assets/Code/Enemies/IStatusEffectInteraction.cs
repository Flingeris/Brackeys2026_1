using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IStatusEffectInteraction
{
    public abstract StatusEffectType Type { get; }
    public int Stacks { get; }
    public void AddStacks(int amount);
    public Sprite GetSprite();
    public string Description { get; }

    public void Tick();
}

public interface IOnTurnEndStatusInteraction : IStatusEffectInteraction, IOnTurnEndStatusTick
{
    IEnumerator OnTurnEndStatusEffect(ICombatEntity entity);
}

public interface IOnTurnEndStatusTick
{
}

public interface IOnDamageTakenStatusTick
{
}

public interface IOnDamageDealtStatusTick
{
}



public interface IDamageModifier : IStatusEffectInteraction
{
    int ModifyDamage(int baseDmg);
}

public interface ITakenDamageFilter : IStatusEffectInteraction, IOnTurnEndStatusTick
{
    int OnBeforeDamageTakenTick(int damage);
}

public interface ITargetFilter : IStatusEffectInteraction, IOnDamageTakenStatusTick
{
    ICombatEntity OnTargetChoose(List<ICombatEntity> aliveMembers, ICombatEntity owner);
}


public enum StatusEffectType
{
    None = 0,
    Bleed = 1,
    Vulnerable = 2,
    Taunt = 3,
    Claws = 4,
}


[Serializable]
public abstract class StatusEffectInteractionBase : IStatusEffectInteraction
{
    public abstract StatusEffectType Type { get; }
    public abstract string Description { get; }

    public int Stacks { get; protected set; } = 0;

    public void AddStacks(int amount)
    {
        Stacks += amount;
    }

    public virtual void Tick()
    {
        Stacks--;
    }


    public Sprite GetSprite()
    {
        switch (Type)
        {
            case StatusEffectType.None:
                break;
            case StatusEffectType.Bleed:
                return "Sprites/Effects/Bleed".Load<Sprite>();
            case StatusEffectType.Vulnerable:
                return "Sprites/Effects/Vuln".Load<Sprite>();
            case StatusEffectType.Taunt:
                return "Sprites/Effects/Taunt".Load<Sprite>();
            case StatusEffectType.Claws:
                return "Sprites/Effects/Claws".Load<Sprite>();
            default:
                throw new ArgumentOutOfRangeException();
        }

        return null;
    }
}

[Serializable]
public class SharpClawsStatusEffect : StatusEffectInteractionBase, IDamageModifier
{
    public int AddBonusForStack = 2;
    public override StatusEffectType Type => StatusEffectType.Claws;
    public int ModifyDamage(int baseDmg)
    {
       var bonus = AddBonusForStack*Stacks;
       return baseDmg + bonus;
    }

    public override void Tick()
    {
    }

    public override string Description { get; }
}


[Serializable]
public class BleedStatusEffect : StatusEffectInteractionBase, IOnTurnEndStatusInteraction
{
    public override StatusEffectType Type => StatusEffectType.Bleed;
    
    public override string Description =>
        "Takes damage at the end of turn equal to its stacks.";

    public IEnumerator OnTurnEndStatusEffect(ICombatEntity entity)
    {
        Debug.Log($"[Bleed] Tick on {entity} for {Stacks} dmg");
        yield return entity.TakeDamage(Stacks);
        yield return null;
    }
}

[Serializable]
public class VulnerabilityStatusEffect : StatusEffectInteractionBase, ITakenDamageFilter
{
    private float multiplier = 1.5f;
    public override StatusEffectType Type => StatusEffectType.Vulnerable;

    public override string Description =>
        "Takes 50% more damage from all sources.";

    public int OnBeforeDamageTakenTick(int damage)
    {
        return Mathf.RoundToInt(damage * multiplier);
    }
}

[Serializable]
public class TauntStatusEffect : StatusEffectInteractionBase, ITargetFilter
{
    public override StatusEffectType Type => StatusEffectType.Taunt;

    public override string Description =>
        "Enemies are forced to target this unit.";

    public ICombatEntity OnTargetChoose(List<ICombatEntity> aliveMembers, ICombatEntity statusOwner)
    {
        return statusOwner;
    }
}