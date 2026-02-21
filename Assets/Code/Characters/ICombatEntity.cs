using System.Collections;
using System.Collections.Generic;

public interface ICombatEntity
{
    int MaxHP { get; }

    // int Speed { get; }
    int CurrShield { get; }
    int CurrHP { get; }
    bool IsDead { get; }
    int CurrPos { get; }

    public void SetTarget(bool b);

    public bool IsPossibleTarget { get; }

    IEnumerator TakeDamage(int amount);
    void Heal(int amount);
    void AddShield(int amount);

    void Kill();

    public void SetPos(int index);

    public IEnumerator EndTurnStatusTick();
    public List<IStatusEffectInteraction> statusEffects { get; }


    public int StatusTypeStacks(StatusEffectType type);

    void AddStatus(IStatusEffectInteraction statusEffect, int stacks);
}