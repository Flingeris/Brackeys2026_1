using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class EnemyInstance : MonoBehaviour
{
    public event UnityAction<int> OnHealthChanged;
    public EnemyModel currModel;
    public int CurrHp { get; private set; }


    [SerializeField] private TMP_Text hpValueText;


    public void SetModel(EnemyModel newModel)
    {
        currModel = newModel;
        if (currModel == null) return;
        CurrHp = currModel.StartingHealth;
    }

    private void UpdateVisuals()
    {
        UpdateHpText();
    }

    private void UpdateHpText()
    {
        hpValueText.SetText("");
        if (currModel == null) return;

        hpValueText.SetText(CurrHp.ToString() + " / " + currModel.StartingHealth.ToString());
    }

    public void TakeDamage(int dmgAmount)
    {
        if (dmgAmount <= 0) return;
        CurrHp = Mathf.Max(0, CurrHp - dmgAmount);
        UpdateVisuals();
    }
}