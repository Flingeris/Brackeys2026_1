using System.Collections.Generic;
using System.Text;
using UnityEngine;

public enum ClassType
{
    None = 0,
    Heal = 1,
    Tank = 2,
    Damage = 3,
}

public enum CardType
{
    None = 0,
    Start = 1,
    Mid = 2,
    End = 3
}


public abstract class CardModel : ContentDef, ITooltipInfo
{
    [Header("Card info")]
    [field: SerializeField]
    public string ItemName { get; protected set; }

    public string Description => GetDescription();

    [Header("Card visuals")] public Sprite Sprite;
    public CardInstance Prefab => "prefabs/Card".Load<CardInstance>();

    [Header("Stats")] public abstract ClassType ClassType { get; }

    public CardType CardType;
    [SerializeReference, SubclassSelector] public List<IOnCardPlayed> OnPlayedCardInteractions;


    [Space(10f)] [SerializeReference, SubclassSelector]
    public List<IOnCardEndTurn> OnTurnEndInteractions;

    public string GetDescription()
    {
        if (OnTurnEndInteractions == null || OnTurnEndInteractions.Count == 0)
            return string.Empty;

        var sb = new StringBuilder();

        for (int i = 0; i < OnTurnEndInteractions.Count; i++)
        {
            var interaction = OnTurnEndInteractions[i];
            if (interaction == null)
                continue;

            if (sb.Length > 0)
                sb.Append('\n');

            sb.Append(interaction.desc);
        }

        return sb.ToString();
    }
}