using UnityEngine;

public abstract class LevelModel : ContentDef
{
    public virtual Sprite backgroundSprite => "Sprites/Levels/Street".Load<Sprite>();
    public virtual SoundId LevelAmbient => SoundId.Ambient_Sewer;
    public EnemyModel[] enemies;
    public int lvlIndex;
}