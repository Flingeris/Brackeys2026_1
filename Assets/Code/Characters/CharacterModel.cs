using UnityEngine;

[CreateAssetMenu(fileName = "New Party Member", menuName = "Party/New Party Member")]
public class CharacterModel : ContentDef
{
    public int MaxHP;
    public ClassType Class;
    public int preferPos;
    public PartyMember Prefab;
    public Sprite sprite;

    public Animation idleAnim;
}