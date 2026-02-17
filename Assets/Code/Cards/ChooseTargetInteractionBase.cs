using System.Collections;

public abstract class ChooseTargetInteractionBase
{
    public TargetSide TargetSide;
    public int[] possiblePositions;

    protected virtual IEnumerator OnTargetChoose()
    {
        yield return G.main.ChooseTarget(TargetSide, possiblePositions);
    }
}