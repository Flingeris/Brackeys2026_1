using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

public abstract class CombatGroup : MonoBehaviour
{
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


    public void DealDamage(int target, int damage)
    {
        var member = partyMembers[target];
        if (member == null || member.IsDead) return;
        member.TakeDamage(damage);
    }

    public void Heal(int target, int amount)
    {
        var member = partyMembers[target];
        if (member == null || member.IsDead) return;
        member.Heal(amount);
    }

    public void AddShield(int target, int amount)
    {
        var member = partyMembers[target];
        if (member == null || member.IsDead) return;
        member.AddShield(amount);
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

    public List<ICombatEntity> GetAliveMembers()
    {
        return partyMembers.Where(p => p != null && !p.IsDead).ToList();
    }


    public bool AllMembersDead()
    {
        return partyMembers.All(m => m == null || m.IsDead);
    }
}