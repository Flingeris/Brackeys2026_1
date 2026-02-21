using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        member.SetPos(index);
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


    public List<ICombatEntity> GetMembersWithStatus(StatusEffectType status)
    {
        if (AllMembersDead()) return null;

        var enemiesWithStatus = new List<ICombatEntity>();

        for (int i = 0; i < partyMembers.Length; i++)
        {
            var m = GetMember(i);
            if (m == null || m.IsDead) continue;

            if (m.StatusTypeStacks(status) > 0)
                enemiesWithStatus.Add(m);
        }

        return enemiesWithStatus;
    }


    private int GetTauntRedirectIndex(int originalIndex)
    {
        var tauntMembers = GetMembersWithStatus(StatusEffectType.Taunt);
        if (tauntMembers == null || tauntMembers.Count == 0)
        {
            return originalIndex;
        }
        
        var tauntIndices = new List<int>();

        foreach (var m in tauntMembers)
        {
            if (m == null || m.IsDead) continue;
            tauntIndices.Add(m.CurrPos);
        }
        
    // var alive = GetAliveMembers();
    //     if (alive == null || alive.Count == 0)
    //         return originalIndex;
    //
    //     // var tauntIndices = new List<int>();
    //
    //
    //     for (int i = 0; i < partyMembers.Length; i++)
    //     {
    //         var m = GetMember(i);
    //         if (m == null || m.IsDead) continue;
    //
    //         if (m.StatusTypeStacks(StatusEffectType.Taunt) > 0)
    //             tauntIndices.Add(i);
    //     }

        if (tauntIndices.Count == 0 || tauntIndices.Contains(originalIndex))
            return originalIndex;

        var index = tauntIndices.GetRandomElement();
        return index;
    }


    public void TargetAll()
    {
        foreach (var member in partyMembers)

        {
            if (member == null) continue;
            member.SetTarget(true);
        }
    }

    public void UntargetAll()
    {
        foreach (var member in partyMembers)
        {
            if (member == null) continue;
            member.SetTarget(false);
        }
    }


    public IEnumerator DealDamage(int target, int damage)
    {
        var finalTarget = GetTauntRedirectIndex(target);
        var combatEntity = GetMember(finalTarget);
        if (combatEntity == null || combatEntity.IsDead) yield break;
      yield return  combatEntity.TakeDamage(damage);
    }

    public IEnumerator DamageAll(int amount)
    {
        foreach (var member in partyMembers)
        {
            if (member == null || member.IsDead) continue;
           yield return member.TakeDamage(amount);
            yield return new WaitForSeconds(0.2f);
        }
    }

    public IEnumerator DamageAllRange(int min, int max)
    {
        foreach (var member in partyMembers)
        {
            if (member == null || member.IsDead) continue;
            var dmg = UnityEngine.Random.Range(min, max + 1);
           yield return member.TakeDamage(dmg);
            yield return new WaitForSeconds(0.2f);
        }
    }


    public void Heal(int target, int amount)
    {
        var member = partyMembers[target];
        if (member == null || member.IsDead) return;
        member.Heal(amount);
    }

    public IEnumerator HealAll(int amount)
    {
        foreach (var member in partyMembers)
        {
            if (member == null || member.IsDead) continue;
            member.Heal(amount);
            yield return new WaitForSeconds(0.2f);
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
            if (member == null || member.IsDead) continue;
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
        var aliveIndices = Enumerable.Range(0, partyMembers.Length)
            .Where(i => partyMembers[i] != null && !partyMembers[i].IsDead)
            .ToList();

        if (aliveIndices.Count == 0)
            return null;

        var originalIndex = aliveIndices.GetRandomElement();

        var finalIndex = GetTauntRedirectIndex(originalIndex);

        return GetMember(finalIndex);
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