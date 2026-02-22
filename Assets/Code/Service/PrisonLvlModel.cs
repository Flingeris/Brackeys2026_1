using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(fileName = "LevelModel", menuName = "Levels/PrisonLvl")]
public class PrisonLvlModel : LevelModel
{
    public override Sprite backgroundSprite => "Sprites/Levels/Prison".Load<Sprite>();
    public override SoundId LevelAmbient => SoundId.Music_Jazz;
    public override VolumeProfile postFx => "PostFX/lvltype2".Load<VolumeProfile>();
}