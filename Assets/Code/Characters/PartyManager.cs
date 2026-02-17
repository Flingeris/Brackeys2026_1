using System.Linq;
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

    public void AddMember(MemberModel model)
    {
        var index = model.preferPos;
        var memberPosition = membersPos[index];
        var member = Instantiate(model.Prefab, memberPosition);
        member.SetModel(model);
        member.CombatGroup = this;
        member.transform.localPosition = Vector3.zero;
        AddMember(member, index);
    }

    public PartyMember GetMemberByClass(ClassType classType)
    {
        return partyMembers
            .OfType<PartyMember>()
            .FirstOrDefault(p => p.state.Class == classType);
    }
}