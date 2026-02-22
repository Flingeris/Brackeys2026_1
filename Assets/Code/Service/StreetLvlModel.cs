using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(fileName = "LevelModel", menuName = "Levels/StreetLvl")]
public class StreetLvlModel : LevelModel
{
    public override Sprite backgroundSprite => "Sprites/Levels/Street".Load<Sprite>();
    public override SoundId LevelAmbient => SoundId.Music_Jazz;
    public override VolumeProfile postFx => "PostFX/lvltype1".Load<VolumeProfile>();
}