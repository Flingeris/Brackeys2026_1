using System;
using System.Threading;
using DG.Tweening;
using TMPro;
using Unity.Collections;
using UnityEngine;


public class MemberState
{
    public MemberModel model;
    public int CurrHP;
    public int MaxHP;
    public int currPos;
    public ClassType Class;
}


public class PartyMember : MonoBehaviour, ICombatEntity
{
    public MemberState state { get; private set; }
    public bool IsDead { get; private set; }
    public int CurrShield { get; private set; }
    public CombatGroup CombatGroup;

    public int MaxHP => state.MaxHP;
    public int CurrHP => state.CurrHP;


    [Header("References")] [SerializeField]
    private TMP_Text hpText;

    [SerializeField] private TMP_Text shieldText;
    [SerializeField] private SpriteRenderer shieldIconSprite;
    [SerializeField] private SpriteRenderer spriteRenderer;


    public void SetState(MemberState newState)
    {
        state = newState;
        UpdateVisuals();
    }

    public void SetModel(MemberModel model)
    {
        if (model == null) return;
        var newState = new MemberState()
        {
            CurrHP = model.MaxHP,
            MaxHP = model.MaxHP,
            model = model,
            currPos = model.preferPos,
            Class = model.Class
        };

        SetState(newState);
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
            state.CurrHP = Mathf.Max(0, state.CurrHP - remainDmg);
            G.audioSystem.Play(SoundId.SFX_PlayerDamaged);
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
        state.CurrHP = Mathf.Min(state.CurrHP + amount, state.MaxHP);
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

    public void OnTurnEnd()
    {
        SetShield(0);
    }

    private void UpdateVisuals()
    {
        UpdateHpVisuals();
        UpdateShieldVisuals();
        UpdateSprite();
    }

    private void UpdateHpVisuals()
    {
        hpText.SetText(state.CurrHP + " / " + state.MaxHP);
    }

    private void UpdateShieldVisuals()
    {
        shieldText.SetText("");
        shieldIconSprite.enabled = false;
        if (CurrShield <= 0) return;
        shieldIconSprite.enabled = true;
        shieldText.SetText(CurrShield.ToString());
    }

    private void UpdateSprite()
    {
        if (spriteRenderer == null)
        {
            Debug.LogError("SpriteRenderer is null! " + this.gameObject.name);
            return;
        }

        spriteRenderer.sprite = state.model.sprite;
    }


    private void CheckIsDead()
    {
        if (state.CurrHP > 0) return;

        IsDead = true;
        CombatGroup.CheckMemberDeath(this);
    }

    public void Kill()
    {
        TakeDamage(CurrHP);
    }
}