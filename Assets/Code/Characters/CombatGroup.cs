using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;


public enum TargetSide
{
    Any = 0,
    Allies = 1,
    Enemies = 2,
}

public abstract class CombatGroup : MonoBehaviour
{
    protected abstract TargetSide Side { get; }
    protected ICombatEntity[] partyMembers;

    [SerializeField] protected Transform[] membersPos;
    public int maxMembersCount = 3;

    protected virtual void Awake()
    {
        partyMembers = new ICombatEntity[maxMembersCount];
    }

    protected void AddMember(ICombatEntity member, int index)
    {
        partyMembers[index] = member;
    }

    public void Clear()
    {
        for (int i = 0; i < partyMembers.Length; i++)
        {
            if (partyMembers[i] is MonoBehaviour mb && mb != null)
            {
                Destroy(mb.gameObject);
            }

            partyMembers[i] = null;
        }
    }

    public ICombatEntity GetMember(int index)
    {
        return partyMembers[index];
    }


    public bool CheckIsAlive(int target)
    {
        return partyMembers[target] != null && !partyMembers[target].IsDead;
    }
    public List<ICombatEntity> GetMembers(int[] positions)
    {
        if (positions == null || positions.Length == 0)
        {
            return GetAliveMembers();
        }

        return positions.Select(i => partyMembers[i]).ToList();
    }


    public void TargetAll()
    {
        foreach (var member in partyMembers)
            member.SetTarget(true);
    }

    public void UntargetAll()
    {
        foreach (var member in partyMembers)
        {
            member.SetTarget(false);
        }
    }


    public void DealDamage(int target, int damage)
    {
        var member = partyMembers[target];
        if (member == null || member.IsDead) return;
        member.TakeDamage(damage);
    }

    public void DamageAll(int amount)
    {
        foreach (var member in partyMembers)
        {
            if (!member.IsDead)
            {
                member.TakeDamage(amount);
            }
        }
    }

    public void Heal(int target, int amount)
    {
        var member = partyMembers[target];
        if (member == null || member.IsDead) return;
        member.Heal(amount);
    }

    public void HealAll(int amount)
    {
        foreach (var member in partyMembers)
        {
            if (!member.IsDead)
                member.Heal(amount);
        }
    }


    public void AddShield(int target, int amount)
    {
        var member = partyMembers[target];
        if (member == null || member.IsDead) return;
        member.AddShield(amount);
    }

    public void ShieldAll(int amount)
    {
        foreach (var member in partyMembers)
        {
            if (!member.IsDead)
                member.AddShield(amount);
        }
    }


    public virtual void CheckMemberDeath(ICombatEntity member)
    {
        if (AllMembersDead())
        {
            OnMembersDeath();
        }
    }

    protected abstract void OnMembersDeath();

    public ICombatEntity GetRandomMember()
    {
        var alive = partyMembers.Where(p => p != null && !p.IsDead).ToList();
        if (alive.Count == 0) return null;
        return alive.GetRandomElement();
    }

    public ICombatEntity GetLowestMember()
    {
        return partyMembers
            .Where(m => m != null && !m.IsDead)
            .OrderBy(m => m.CurrHP)
            .FirstOrDefault();
    }


    public List<ICombatEntity> GetAliveMembers()
    {
        return partyMembers.Where(p => p != null && !p.IsDead).ToList();
    }


    public bool AllMembersDead()
    {
        return partyMembers.All(m => m == null || m.IsDead);
    }
}