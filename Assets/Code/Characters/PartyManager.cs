using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

public class PartyManager : MonoBehaviour
{
    [SerializeField] private PartyMember[] partyMembers;
    [SerializeField] private Transform[] membersPos;
    public int maxMembersCount = 3;

    private void Awake()
    {
        G.party = this;
        partyMembers = new PartyMember[maxMembersCount];
    }

    public void AddMember(MemberModel model)
    {
        var index = model.preferPos;
        var memberPosition = membersPos[index];
        var member = Instantiate(model.Prefab, memberPosition);
        member.SetModel(model);
        member.transform.localPosition = Vector3.zero;
        partyMembers[index] = member;
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


    public void OnMemberDeath(PartyMember member)
    {
        if (AllMembersDead())
        {
            G.main.GameLost();
        }
    }

    public PartyMember GetRandomMember()
    {
        var alive = partyMembers.Where(p => p != null && !p.IsDead).ToList();
        if (alive.Count == 0) return null;
        return alive.GetRandomElement();
    }

    public List<PartyMember> GetAliveMembers()
    {
        return partyMembers.Where(p => p != null && !p.IsDead).ToList();
    }

    public PartyMember GetMemberByClass(ClassType classType)
    {
        return partyMembers.FirstOrDefault(p => p.state.Class == classType);
    }


    public bool AllMembersDead()
    {
        return partyMembers.All(m => m == null || m.IsDead);
    }
}