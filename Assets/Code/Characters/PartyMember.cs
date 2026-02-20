using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using Random = UnityEngine.Random;


public class MemberState
{
    public CharacterModel model;
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

    public List<IStatusEffectInteraction> statusEffects { get; set; } = new();

    [Header("References")] [SerializeField]
    private TMP_Text hpText;

    [SerializeField] private TMP_Text shieldText;
    [SerializeField] private SpriteRenderer shieldIconSprite;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private SpriteRenderer highlight;

    [SerializeField] private SpriteRenderer statusEffectsIcons;
    [SerializeField] private TMP_Text statusEffectsText;

    [SerializeField] private HpBarView hpBarView;

    [Header("Popup")] [SerializeField] private float popupOffsetY = 1.5f;


    private void Start()
    {
        UpdateVisuals();
    }

    public int StatusTypeStacks(StatusEffectType type)
    {
        var stacks = 0;
        foreach (var statusEffectInteraction in statusEffects)
        {
            if (statusEffectInteraction.Type == type)
            {
                stacks += statusEffectInteraction.Stacks;
            }
        }

        return stacks;
    }

    public void AddStatus(IStatusEffectInteraction statusEffect, int stacks)
    {
        var existing = statusEffects.FirstOrDefault(s => s.GetType() == statusEffect.GetType());

        if (existing != null)
        {
            existing.AddStacks(stacks);
            Debug.Log($"Added stack {stacks} to {statusEffect.GetType()}");
        }
        else
        {
            var instance = (IStatusEffectInteraction)Activator.CreateInstance(statusEffect.GetType());
            instance.AddStacks(stacks);
            statusEffects.Add(instance);
        }

        UpdateStatusIcon();
    }

    private void UpdateStatusIcon()
    {
        statusEffects.RemoveAll(s => s.Stacks <= 0);


        statusEffectsIcons.enabled = false;
        statusEffectsText.SetText("");

        var effect = statusEffects.FirstOrDefault();
        if (effect == null || effect.Type == StatusEffectType.None) return;

        var sprite = effect.GetSprite();
        if (sprite == null) return;

        statusEffectsIcons.enabled = true;
        statusEffectsIcons.sprite = sprite;

        if (effect.Stacks != 0)
        {
            statusEffectsText.SetText(effect.Stacks.ToString());
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

    public void SetPos(int pos)
    {
        state.currPos = pos;
    }

    public void SetModel(CharacterModel model)
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
        int shownDamage = dmgAmount;

        if (CurrShield > 0)
        {
            int absorbed = Mathf.Min(CurrShield, remainDmg);
            CurrShield -= absorbed;
            remainDmg -= absorbed;
        }

        if (remainDmg > 0)
        {
            state.CurrHP = Mathf.Max(0, state.CurrHP - remainDmg);
            float pitch = Random.Range(0.8f, 1.2f);
            G.audioSystem.PlayPitched(SoundId.SFX_PlayerDamaged, pitch);
        }
        else
        {
            G.audioSystem.Play(SoundId.SFX_DamageBlocked);
        }

        if (shownDamage > 0 && G.textPopup != null)
            G.textPopup.SpawnAbove(transform, popupOffsetY, shownDamage, isHeal: false);

        transform.DOPunchScale(new Vector3(0.2f, 0.2f, 0.2f), 0.2f);
        CheckIsDead();
        UpdateVisuals();
    }

    public void Heal(int amount)
    {
        if (IsDead) return;
        state.CurrHP = Mathf.Min(state.CurrHP + amount, state.MaxHP);
        UpdateVisuals();

        if (G.textPopup != null)
            G.textPopup.SpawnAbove(transform, popupOffsetY, amount, isHeal: true);

        G.audioSystem.Play(SoundId.SFX_PlayerHealed);
    }

    public void AddShield(int amount)
    {
        if (IsDead) return;
        CurrShield += amount;
        UpdateVisuals();

        G.audioSystem.Play(SoundId.SFX_PlayerShielded);
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
        UpdateStatusIcon();
    }

    private void UpdateHpVisuals()
    {
        hpText.SetText(state.CurrHP + "/" + state.MaxHP);

        if (hpBarView != null && state.MaxHP > 0)
        {
            float normalized = (float)state.CurrHP / state.MaxHP;
            hpBarView.SetNormalized(normalized);
        }
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