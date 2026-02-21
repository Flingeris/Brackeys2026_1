using UnityEngine;

public class LevelProgressView : MonoBehaviour
{
    [SerializeField] private RectTransform[] levelsPoint;
    [SerializeField] private RectTransform movingHead;


    private void Start()
    {
        SetLvlVisuals(G.run.mapNodeIndex);
    }


    private void MoveTo(int lvlIndex)
    {
        // movingHead.DOKill();
        // movingHead.DOLocalMove(levelsPoint[CurrentIndex].localPosition, 0.65f).SetEase(Ease.InOutBack);
    }

    private void SetLvlVisuals(int index)
    {
        movingHead.anchoredPosition = levelsPoint[index].anchoredPosition;
    }
}