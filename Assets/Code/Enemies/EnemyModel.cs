using UnityEngine;

[CreateAssetMenu(fileName = "EnemyModel", menuName = "Enemies/EnemyModel")]
public class EnemyModel : ContentDef, ITooltipInfo
{
    [Header("Enemy info")] public string ItemName { get; }
    public string Description { get; }

    [Header("Enemy Visuals")] public Sprite Sprite;

    public EnemyInstance Prefab => "prefabs/Enemy".Load<EnemyInstance>();

    [Header("Stats")]
    public int StartingHealth;

    public int Dmg;
}