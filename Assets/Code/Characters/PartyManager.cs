using System.Linq;
using DG.Tweening;
using UnityEngine;

public class PartyManager : CombatGroup
{
    protected override TargetSide Side => TargetSide.Allies;

    protected override void Awake()
    {
        base.Awake();
        G.party = this;
    }

    protected override void OnMembersDeath()
    {
        G.main.GameLost();
    }

    public PartyMember CreateMember(CharacterModel model)
    {
        var index = model.preferPos;
        var member = Instantiate(model.Prefab);
        member.SetModel(model);
        member.SetPos(index);
        return member;
    }

    public void AddMember(PartyMember member, int index)
    {
        member.CombatGroup = this;
        var memberPosition = membersPos[index];
        member.transform.SetParent(memberPosition, false);
        member.transform.localPosition = Vector3.zero;
        partyMembers[index] = member;
    }

    public PartyMember GetMemberByClass(ClassType classType)
    {
        return partyMembers
            .OfType<PartyMember>()
            .FirstOrDefault(p => p.state.Class == classType);
    }
}