using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class EnemyInstance : MonoBehaviour
{
    public event UnityAction<int> OnHealthChanged;
    public EnemyModel currModel;
    public int CurrHp { get; private set; }
    public bool IsDead { get; private set; }
    public int CurrDmg { get; set; }


    [SerializeField] private TMP_Text hpValueText;


    public void SetModel(EnemyModel newModel)
    {
        currModel = newModel;
        if (currModel == null) return;
        CurrHp = currModel.StartingHealth;
        CurrDmg = currModel.Dmg; 
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
        CheckIsDead();
        UpdateVisuals();
    }

    private void CheckIsDead()
    {
        if (CurrHp <= 0)
        {
            IsDead = true;
            Destroy(this.gameObject);
        }
    }
}