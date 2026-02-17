using UnityEngine;

[CreateAssetMenu(fileName = "HealerCard", menuName = "Cards/HealerCard", order = 1)]
public class HealerCard : CardModel
{
    public override ClassType ClassType => ClassType.Heal;
}