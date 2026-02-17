using UnityEngine;

[CreateAssetMenu(fileName = "DamageCard", menuName = "Cards/DamageCard", order = 3)]
public class DamageCard : CardModel
{
    public override ClassType ClassType => ClassType.Damage;
}