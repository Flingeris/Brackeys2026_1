using UnityEngine;

[CreateAssetMenu(fileName = "TankCard", menuName = "Cards/TankCard", order = 2)]
public class TankCard : CardModel
{
    public override ClassType ClassType => ClassType.Tank;
}