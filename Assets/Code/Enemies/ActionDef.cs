using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;


public enum InteractionType
{
    None = 0,
    Attack = 1,
    Shield = 2,
    Heal = 3,
    Debuff = 4,
    Buff = 5
}


[Serializable]
public class ActionDef : ITooltipInfo
{
    public virtual InteractionType Type => GetActionType();
    [SerializeReference, SubclassSelector] public List<IOnEnemyTurnEnd> OnEndTurnInteractions;
    [HideInInspector] public string Amount;

    private InteractionType GetActionType()
    {
        foreach (var interaction in OnEndTurnInteractions)
        {
            if (interaction.type == InteractionType.None) continue;
            if (interaction is IAmountInteraction amountInteraction)
            {
                Amount = amountInteraction.GetAmountAsString();
            }

            return interaction.type;
        }

        return InteractionType.None;
    }

    public string GetDescription()
    {
        if (OnEndTurnInteractions == null || OnEndTurnInteractions.Count == 0)
            return string.Empty;

        var sb = new StringBuilder();

        for (int i = 0; i < OnEndTurnInteractions.Count; i++)
        {
            var interaction = OnEndTurnInteractions[i];
            if (interaction == null)
                continue;

            if (sb.Length > 0)
                sb.Append('\n');

            sb.Append(interaction.desc);
        }

        return sb.ToString();
    }

    public string GetName()
    {
        if (OnEndTurnInteractions == null || OnEndTurnInteractions.Count == 0)
            return string.Empty;

        return OnEndTurnInteractions[0].GetType().Name;
    }


    public static Sprite GetSprite(InteractionType type)
    {
        switch (type)
        {
            case InteractionType.None:
                break;
            case InteractionType.Attack:
                return "Sprites/Actions/Attack".Load<Sprite>();
            case InteractionType.Shield:
                return "Sprites/Actions/Shield".Load<Sprite>();
            case InteractionType.Heal:
                return "Sprites/Actions/Heal".Load<Sprite>();
            case InteractionType.Debuff:
                return "Sprites/Actions/Debuff".Load<Sprite>();
            case InteractionType.Buff:
                return "Sprites/Actions/Buff".Load<Sprite>();
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }

        return null;
    }

    public string ItemName => string.Empty;
    public string Description => GetDescription();
}