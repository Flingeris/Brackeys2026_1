using System.Linq;
using System.Text;
using UnityEngine;

public class EnemyStatusTooltipGiver : MonoBehaviour, ITooltipInfoGiver
{
    [SerializeField] private EnemyInstance enemy;

    private void Awake()
    {
        if (enemy == null)
            enemy = GetComponentInParent<EnemyInstance>();
    }

    public ITooltipInfo GetTooltipInfo()
    {
        if (enemy == null)
            return null;
        
        var activeEffects = enemy.statusEffects
            .Where(s => s != null && s.Stacks > 0 && s.Type != StatusEffectType.None)
            .ToList();

        if (activeEffects.Count == 0)
            return null;

        var sb = new StringBuilder();

        foreach (var effect in activeEffects)
        {
            if (sb.Length > 0)
                sb.Append("\n\n");

            var statusName = GetStatusName(effect.Type);

            if (effect.Stacks > 0)
                sb.Append($"{statusName} x{effect.Stacks}");
            else
                sb.Append(statusName);
        }
        
        return new TooltipStatusEffect(string.Empty, sb.ToString());
    }

    private static string GetStatusName(StatusEffectType type)
    {
        switch (type)
        {
            case StatusEffectType.Bleed:
                return "Bleed";
            case StatusEffectType.Vulnerable:
                return "Vulnerable";
            case StatusEffectType.Taunt:
                return "Taunt";
            default:
                return type.ToString();
        }
    }
}