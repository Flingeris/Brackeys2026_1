using System;
using System.Collections;

public interface ICardInteraction
{
}


public interface IOnCardPlayed : ICardInteraction
{
    public string desc { get; }
    public IEnumerator OnPlay(CardState card);
}


[Serializable]
public class ChooseTargetInteraction : ChooseTargetInteractionBase, IOnCardPlayed, INoAnimationAction
{
    public string desc => string.Empty;

    public IEnumerator OnPlay(CardState state)
    {
        yield return G.main.ChooseTarget(TargetSide, possiblePositions);
    }
}