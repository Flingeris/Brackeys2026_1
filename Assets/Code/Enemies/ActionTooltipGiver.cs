using UnityEngine;

public class ActionTooltipGiver : MonoBehaviour, ITooltipInfoGiver
{
    [SerializeField] private EnemyInstance enemy;

    private void OnValidate()
    {
        if (enemy == null) enemy.GetComponentInParent<EnemyInstance>();
    }

    public ITooltipInfo GetTooltipInfo()
    {
        return enemy.nextAction;
    }
}