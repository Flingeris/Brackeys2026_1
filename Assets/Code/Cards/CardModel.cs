using System;
using System.Collections.Generic;
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


[CreateAssetMenu(fileName = "CardModel", menuName = "Cards/CardModel", order = 1)]
public class CardModel : ContentDef, ITooltipInfo
{
    [Header("Card info")]
    [field: SerializeField]
    public string ItemName { get; }

    [field: SerializeField] public string Description { get; }

    [Header("Card visuals")] public Sprite CardSprite;
    public CardInstance Prefab => "prefabs/Card".Load<CardInstance>();
    [Header("Stats")] public ClassType ClassType;
    public CardType CardType;
    [SerializeReference, SubclassSelector] public List<CardLogic> CardLogic = new();
}

[Serializable]
public abstract class CardLogic
{
    public virtual CardType cardType { get; protected set; }

    public bool Use()
    {
        if (!CanBeUsed()) return false;
        OnUse();
        return true;
    }

    protected abstract void OnUse();

    protected virtual bool CanBeUsed()
    {
        return true;
    }
}
[Serializable]
public abstract class StartCardLogic : CardLogic
{
    public override CardType cardType => CardType.Start;

    protected override void OnUse()
    {
        throw new NotImplementedException();
    }
}
[Serializable]
public abstract class MidCardLogic : CardLogic
{
    protected override void OnUse()
    {
        throw new NotImplementedException();
    }
}
[Serializable]
public abstract class EndCarLogic : CardLogic
{
    protected override void OnUse()
    {
        throw new NotImplementedException();
    }
}

[Serializable]
public class StartHealAll : StartCardLogic
{
    protected override void OnUse()
    {
        throw new NotImplementedException();
    }
}