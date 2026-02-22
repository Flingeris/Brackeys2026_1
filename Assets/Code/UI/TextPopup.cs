using UnityEngine;

public class TextPopupManager : MonoBehaviour
{
    public TextPopupInstance popupPrefab;

    private void Awake()
    {
        G.textPopup = this;
    }

    public void Spawn(Vector3 worldPos, int amount, bool isHeal = false, bool isMaxHpLoss = false)
    {
        if (!popupPrefab)
        {
            Debug.LogWarning("TextPopupManager: popupPrefab is null");
            return;
        }

        var inst = Instantiate(popupPrefab, worldPos, Quaternion.identity);
        inst.Play(amount, isHeal, isMaxHpLoss);
    }

    public void SpawnAbove(Transform anchor, float offsetY, int amount, bool isHeal = false, bool isMaxHpLoss = false)
    {
        if (!anchor) return;
        Spawn(anchor.position + Vector3.up * offsetY, amount, isHeal, isMaxHpLoss);
    }
}