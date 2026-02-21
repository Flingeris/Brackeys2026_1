using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyGroup : CombatGroup
{
    protected override TargetSide Side => TargetSide.Enemies;

    public void AddEnemy(EnemyModel model, int index = -1)
    {
        //TODO - check array bound
        if (index == -1)
        {
            index = model.preferPos;
        }


        if (index == -1)
            index = Array.FindIndex(partyMembers, m => m == null);

        if (index < 0) return;

        var memberPosition = membersPos[index];
        var member = Instantiate(model.Prefab, memberPosition);
        member.SetModel(model);
        member.combatGroup = this;
        member.transform.localPosition = Vector3.zero;
        AddMember(member, index);
    }


    protected override void OnMembersDeath()
    {
    }

    public List<EnemyInstance> GetAliveEnemies()
    {
        return GetAliveMembers()
            .Select(e => e as EnemyInstance)
            .Where(e => e != null)
            .ToList();
    }


    protected override void Awake()
    {
        base.Awake();
        G.enemies = this;
    }
}