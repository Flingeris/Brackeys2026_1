using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;

public interface IActionAnimation
{
    IEnumerator Play(Transform transform);
}


public interface INoAnimationAction
{
}


[Serializable]
public abstract class ActionAnimationBase : IActionAnimation
{
    public abstract IEnumerator Play(Transform transform);

    // public static ActionAnimationBase Create(InteractionType type)
    // {
    //     
    // }
}

public interface ICardAnimation
{
}

public interface ICharacterAnimation
{
}


[Serializable]
public class CharacterAttackAnimation : ActionAnimationBase, ICharacterAnimation
{
    public override IEnumerator Play(Transform transform)
    {
        var ownerStartPos = transform.position;
        var ownerEndPos = ownerStartPos;
        ownerEndPos.x += 3f;

        transform.DOMove(ownerEndPos, 0.2f);
        yield return new WaitForSeconds(0.2f);
        transform.DOMove(ownerStartPos, 0.2f);
        yield return new WaitForSeconds(0.2f);
    }
}


[Serializable]
public class CardActivationAnimation : ActionAnimationBase, ICardAnimation
{
    public override IEnumerator Play(Transform transform)
    {
        
        var startPos = transform.position;
        var NewPos = startPos;
        NewPos.y += 0.8f;
        yield return transform.DOMove(NewPos, 0.2f).WaitForCompletion();


        transform.DOShakePosition(0.5f, 0.2f, 40);
        yield return new WaitForSeconds(0.5f);
        yield return transform.DOMove(startPos, 0.2f).WaitForCompletion();
    }
}