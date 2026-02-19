using System;
using System.Collections.Generic;
using System.Threading;
using DG.Tweening;
using TMPro;
using Unity.Collections;
using UnityEngine;
using UnityEngine.EventSystems;


public class MemberState
{
    public MemberModel model;
    public int CurrHP;
    public int MaxHP;
    public int currPos;
    public ClassType Class;
}


public class PartyMember : MonoBehaviour, ICombatEntity, IPointerClickHandler
{
    public MemberState state { get; private set; }
    public bool IsDead { get; private set; }
    public int CurrShield { get; private set; }
    public bool IsPossibleTarget { get; private set; }
    public CombatGroup CombatGroup;

    public int MaxHP => state.MaxHP;
    public int CurrHP => state.CurrHP;

    public List<IStatusEffectInteraction> statusEffects;

    [Header("References")] [SerializeField]
    private TMP_Text hpText;

    [SerializeField] private TMP_Text shieldText;
    [SerializeField] private SpriteRenderer shieldIconSprite;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private SpriteRenderer highlight;


    public int StatusTypeStacks(StatusEffectType type)
    {
        throw new NotImplementedException();
    }

    public void AddStatus(IStatusEffectInteraction statusEffect, int stacks)
    {
        if (statusEffects.Contains(statusEffect))
        {
            var index = statusEffects.IndexOf(statusEffect);
            statusEffects[index].AddStacks(stacks);
        }
        else
        {
            statusEffects.Add(statusEffect);
            statusEffect.AddStacks(stacks);
        }
    }


    public void SetTarget(bool b)
    {
        IsPossibleTarget = b;
    }

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

    private void Update()
    {
        SetHighlight(IsPossibleTarget);
    }

    private void SetHighlight(bool b)
    {
        if (highlight == null) return;
        highlight.gameObject.SetActive(b);
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

    public void OnPointerClick(PointerEventData eventData)
    {
        G.main.TryChooseTarget(this);
    }
}