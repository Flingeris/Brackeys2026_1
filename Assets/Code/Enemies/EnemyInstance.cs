using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class EnemyInstance : MonoBehaviour, ICombatEntity
{
    public EnemyModel currModel;
    public int MaxHP { get; private set; }
    public int CurrHP { get; private set; }
    public bool IsDead { get; private set; }
    public int CurrShield { get; private set; }
    public int CurrDmg { get; private set; }

    public CombatGroup combatGroup;


    [SerializeField] private TMP_Text hpValueText;


    public void SetModel(EnemyModel newModel)
    {
        MaxHP = newModel.StartingHealth;
        currModel = newModel;
        if (currModel == null) return;
        CurrHP = currModel.StartingHealth;
        CurrDmg = currModel.Dmg;
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        UpdateHpText();
    }

    private void UpdateHpText()
    {
        hpValueText.SetText("");
        if (currModel == null) return;

        hpValueText.SetText(CurrHP.ToString() + " / " + currModel.StartingHealth.ToString());
    }

    public void TakeDamage(int dmgAmount)
    {
        if (IsDead) return;
        if (dmgAmount <= 0) return;

        var remainDmg = dmgAmount;

        if (CurrShield > 0)
        {
            int absorbed = Mathf.Min(CurrShield, remainDmg);
            CurrShield -= absorbed;
            remainDmg -= absorbed;
        }

        if (remainDmg > 0)
        {
            CurrHP = Mathf.Max(0, CurrHP - remainDmg);
            // G.audioSystem.Play(SoundId.SFX_PlayerDamaged);
        }
        else
        {
            G.audioSystem.Play(SoundId.SFX_DamageBlocked);
        }

        transform.DOPunchScale(new Vector3(0.2f, 0.2f, 0.2f), 0.2f);
        CheckIsDead();
        UpdateVisuals();
    }

    public void Heal(int amount)
    {
        if (IsDead) return;
        CurrHP = Mathf.Min(CurrHP + amount, MaxHP);
        UpdateVisuals();
    }

    public void AddShield(int amount)
    {
        if (IsDead) return;
        CurrShield += amount;
        UpdateVisuals();
    }

    public void SetShield(int amount)
    {
        if (IsDead) return;
        CurrShield = 0;
        AddShield(amount);
    }

    private void CheckIsDead()
    {
        if (CurrHP <= 0)
        {
            IsDead = true;
            combatGroup.CheckMemberDeath(this);
            Destroy(gameObject);
        }
    }

    public void Kill()
    {
        TakeDamage(CurrHP);
    }
}