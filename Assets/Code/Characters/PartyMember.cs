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
    public readonly CharacterModel model;
    public int CurrHP;
    public int MaxHP;
    public int currPos;
    public readonly ClassType Class;

    public MemberState(CharacterModel model)
    {
        CurrHP = model.MaxHP;
        MaxHP = model.MaxHP;
        this.model = model;
        currPos = model.preferPos;
        Class = model.Class;
    }
}


public class PartyMember : MonoBehaviour, ICombatEntity, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler

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
    [SerializeField] private SpriteRenderer shieldHpBarFrame;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private SpriteRenderer highlight;

    [SerializeField] private SpriteRenderer statusEffectsIcons;
    [SerializeField] private TMP_Text statusEffectsText;

    [SerializeField] private HpBarView hpBarView;

    [Header("Popup")] [SerializeField] private float popupOffsetY = 1.5f;

[Header("Target highlight")]
    [SerializeField] private Color targetColor;
    [SerializeField] private Color hoverColor;
    [SerializeField] private float targetScale = 1f;
    [SerializeField] private float hoverScale = 1.1f;

    [Header("Target pulse")]
    [SerializeField] private float pulseScaleMultiplier = 1.1f;
    [SerializeField] private float pulseDuration = 0.4f;
    [SerializeField] private float pulseMinAlpha = 0.3f;
    [SerializeField] private float pulseMaxAlpha = 0.8f;

    private bool isHovered;

    private Vector3 highlightBaseScale;
    private Tween pulseScaleTween;
    private Tween pulseColorTween;
    
    private void Awake()
    {
        if (highlight != null)
        {
            highlightBaseScale = highlight.transform.localScale;
            highlight.gameObject.SetActive(false);
        }
    }
    
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
        RefreshHighlight();
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        RefreshHighlight();
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        RefreshHighlight();
    }

    public void SetState(MemberState newState)
    {
        state = newState;
        state.CurrHP = state.MaxHP;
        UpdateVisuals();
    }

    public void SetPos(int pos)
    {
        state.currPos = pos;
    }

    public void SetModel(CharacterModel model)
    {
        if (model == null) return;
        var newState = new MemberState(model);

        SetState(newState);
    }
    private void RefreshHighlight()
    {
        if (highlight == null) return;
        
        if (!G.main.IsChoosingTarget || !IsPossibleTarget)
        {
            StopTargetPulse();
            return;
        }
        
        if (isHovered)
        {
            pulseScaleTween?.Kill();
            pulseColorTween?.Kill();
            pulseScaleTween = null;
            pulseColorTween = null;

            highlight.gameObject.SetActive(true);
            highlight.transform.localScale = highlightBaseScale * hoverScale;
            highlight.color = hoverColor;
        }
        else
        {
            StartTargetPulse();
        }
    }
    
    private void StartTargetPulse()
    {
        if (highlight == null) return;
        
        if (pulseScaleTween != null && pulseScaleTween.IsActive())
            return;
        
        pulseScaleTween?.Kill();
        pulseColorTween?.Kill();

        highlight.gameObject.SetActive(true);
        highlight.transform.localScale = highlightBaseScale;
        
        Color startColor = targetColor;
        startColor.a = pulseMinAlpha;
        highlight.color = startColor;

        pulseScaleTween = highlight.transform
            .DOScale(highlightBaseScale * pulseScaleMultiplier, pulseDuration)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine)
            .SetUpdate(true);

        Color endColor = targetColor;
        endColor.a = pulseMaxAlpha;

        pulseColorTween = highlight
            .DOColor(endColor, pulseDuration)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine)
            .SetUpdate(true);
    }

    private void StopTargetPulse()
    {
        if (highlight == null) return;

        pulseScaleTween?.Kill();
        pulseColorTween?.Kill();
        pulseScaleTween = null;
        pulseColorTween = null;

        highlight.gameObject.SetActive(false);
        highlight.transform.localScale = highlightBaseScale;
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
        shieldHpBarFrame.enabled = false;
        if (CurrShield <= 0) return;
        shieldIconSprite.enabled = true;
        shieldHpBarFrame.enabled = true;
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
        state.MaxHP = Mathf.Max(4, state.MaxHP - 4);
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