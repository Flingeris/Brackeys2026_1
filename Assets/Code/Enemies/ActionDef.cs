using System;
using System.Collections.Generic;
using UnityEngine;


public enum InteractionType
{
    None = 0,
    Attack = 1,
    Shield = 2,
    Heal = 3
}


[Serializable]
public class ActionDef
{
    public virtual InteractionType Type => GetActionType();
    [SerializeReference, SubclassSelector] public List<IOnEnemyTurnEnd> OnEndTurnInteractions;
    [HideInInspector] public int Amount;

    private InteractionType GetActionType()
    {
        foreach (var interaction in OnEndTurnInteractions)
        {
            if (interaction.type == InteractionType.None) continue;
            if (interaction is IAmountInteraction amountInteraction)
            {
                Amount = amountInteraction.GetAmount();
            }

            return interaction.type;
        }

        return InteractionType.None;
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
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }

        return null;
    }
}