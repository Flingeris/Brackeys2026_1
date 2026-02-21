using System;
using System.Collections;

public interface IOnCardEndTurn : ICardInteraction
{
    public string desc { get; }
    public IEnumerator OnEndTurn(CardState card);
}


[Serializable]
public class ChooseTargetOnTurnEndInteraction : ChooseTargetInteractionBase, IOnCardEndTurn, INoAnimationAction
{
    public string desc => string.Empty;

    public IEnumerator OnEndTurn(CardState state)
    {
        yield return OnTargetChoose();
    }
}