using UnityEngine;

[CreateAssetMenu(fileName = "LevelModel", menuName = "Levels/StreetLvl")]
public class StreetLvlModel : LevelModel
{
    public override Sprite backgroundSprite => "Sprites/Levels/Street".Load<Sprite>();
    public override SoundId LevelAmbient => SoundId.Ambient_Sewer;
}