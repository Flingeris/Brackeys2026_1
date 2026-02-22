using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using Unity.IntegerTime;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.EventSystems;

public class EnemyInstance : MonoBehaviour,
    ITurnEntity, ICombatEntity,
    IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public EnemyModel model;
    public int Speed { get; private set; }
    public int MaxHP { get; private set; }
    public int TurnOrder { get; private set; }
    public int CurrHP { get; private set; }
    public bool IsDead { get; private set; }
    public int CurrShield { get; private set; }
    public bool IsPossibleTarget { get; private set; }
    public int CurrTurnIndex = -1;
    public int CurrPos { get; private set; }


    public ICombatEntity currentTarget;

    public List<IStatusEffectInteraction> statusEffects { get; set; } = new();

    public ActionDef nextAction;
    public CombatGroup combatGroup;

    [SerializeField] private BoxCollider2D col;

    [Header("Visual References")] [SerializeField]
    private TMP_Text hpValueText;

    [SerializeField] private SpriteRenderer highlight;
    [SerializeField] private SpriteRenderer sprite;
    [SerializeField] private TMP_Text turnIndexText;
    [SerializeField] private SpriteRenderer actionIconImage;
    [SerializeField] private TMP_Text actionValueText;

    [SerializeField] private Transform statusIconsRoot;
    [SerializeField] private StatusIconView statusIconPrefab;
    private readonly List<StatusIconView> statusIconViews = new();
    [SerializeField] private HpBarView hpBarView;

    [SerializeField] private SpriteRenderer shieldIconImage;
    [SerializeField] private SpriteRenderer shieldHpBarFrame;
    [SerializeField] private TMP_Text shieldValueText;


    [Header("Target highlight")] [SerializeField]
    private Color targetColor;

    [SerializeField] private Color hoverColor;
    [SerializeField] private float targetScale;
    [SerializeField] private float hoverScale;

    [Header("Target pulse")] [SerializeField]
    private float pulseScaleMultiplier;

    [SerializeField] private float pulseDuration;
    [SerializeField] private float pulseMinAlpha;
    [SerializeField] private float pulseMaxAlpha;
    [Header("Popup")] [SerializeField] private float popupOffsetY = 1.5f;

    [Header("Visual Root")] [SerializeField]
    private Transform visualRoot;

    private Tween highlightTween;
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


    public int GetFinalDamage(int damage)
    {
        var finalDmg = damage;
        foreach (var statusEffectInteraction in statusEffects)
        {
            if (statusEffectInteraction is IDamageModifier dmgMod)
            {
                finalDmg = dmgMod.ModifyDamage(finalDmg);
            }

            if (statusEffectInteraction is IOnDamageDealtStatusTick)
            {
                statusEffectInteraction.Tick();
            }
        }

        return finalDmg;
    }

    private void Start()
    {
        UpdateVisuals();
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

    public void SetModel(EnemyModel newModel)
    {
        MaxHP = newModel.StartingHealth;
        model = newModel;
        if (model == null) return;
        Speed = model.Speed;
        CurrHP = model.StartingHealth;
        UpdateVisuals();

        var b = sprite.sprite.bounds;
        col.size = b.size;
        col.offset = b.center;
    }

    private void UpdateVisuals()
    {
        if (IsDead) return;
        if (IsDead) return;
        UpdateHpText();
        UpdateSprite();
        UpdateNextActionIcon();
        UpdateStatusIcon();
        UpdateShieldVisuals();
    }


    private void UpdateShieldVisuals()
    {
        shieldValueText.SetText("");
        shieldIconImage.enabled = false;
        shieldHpBarFrame.enabled = false;
        if (CurrShield <= 0) return;
        shieldIconImage.enabled = true;
        shieldHpBarFrame.enabled = true;
        shieldValueText.SetText(CurrShield.ToString());
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


    private void UpdateNextActionIcon()
    {
        actionIconImage.enabled = false;
        actionValueText.SetText("");

        nextAction = GetAction(CurrTurnIndex + 1, model.EndTurnActions);
        if (nextAction == null || nextAction.Type == InteractionType.None) return;

        var actionSprite = ActionDef.GetSprite(nextAction.Type);
        if (sprite == null) return;

        actionIconImage.enabled = true;
        actionIconImage.sprite = actionSprite;

        if (nextAction.Amount != "")
        {
            actionValueText.SetText(nextAction.Amount.ToString());
        }
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

    private void UpdateStatusIcon()
    {
        if (statusIconsRoot == null || statusIconPrefab == null)
            return;
        
        foreach (var view in statusIconViews)
        {
            if (view != null)
                view.gameObject.SetActive(false);
        }
        
        var activeEffects = statusEffects
            .Where(s => s != null && s.Stacks > 0 && s.Type != StatusEffectType.None)
            .ToList();

        if (activeEffects.Count == 0)
            return;

        const float spacing = 0.3f;

        for (int i = 0; i < activeEffects.Count; i++)
        {
            var effect = activeEffects[i];
            var sprite = effect.GetSprite();
            if (sprite == null)
                continue;
            
            if (i >= statusIconViews.Count || statusIconViews[i] == null)
            {
                var inst = Instantiate(statusIconPrefab, statusIconsRoot);
                statusIconViews.Add(inst);
            }

            var view = statusIconViews[i];
            view.gameObject.SetActive(true);
            view.Setup(sprite, effect.Stacks);
            
            view.transform.localPosition = new Vector3(i * spacing, 0f, 0f);
        }
    }

    private void UpdateSprite()
    {
        if (sprite == null) return;
        sprite.sprite = model.Sprite;
    }

    private void UpdateHpText()
    {
        hpValueText.SetText("");
        if (model == null) return;

        hpValueText.SetText(CurrHP.ToString() + "/" + model.StartingHealth.ToString());
        if (hpBarView != null && model.StartingHealth > 0)
        {
            float normalized = (float)CurrHP / model.StartingHealth;
            hpBarView.SetNormalized(normalized);
        }
    }


    public IEnumerator TakeDamage(int dmgAmount)
    {
        if (IsDead) yield break;
        if (dmgAmount <= 0) yield break;

        foreach (var statusEffectInteraction in statusEffects)
        {
            if (statusEffectInteraction is ITakenDamageFilter dmgFilter)
            {
                dmgAmount = dmgFilter.OnBeforeDamageTakenTick(dmgAmount);
            }
        }


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
            CurrHP = Mathf.Max(0, CurrHP - remainDmg);
            G.audioSystem.Play(SoundId.SFX_EnemyDamaged);
        }
        else
        {
            G.audioSystem.Play(SoundId.SFX_DamageBlocked);
        }

        if (shownDamage > 0 && G.textPopup != null)
            G.textPopup.SpawnAbove(transform, popupOffsetY, shownDamage, isHeal: false);

        transform.DOPunchScale(new Vector3(0.2f, 0.2f, 0.2f), 0.2f);
        visualRoot.transform.DOShakePosition(0.5f, 0.15f, 40);

        yield return OnDamageStatusTick();
        CheckIsDead();
        UpdateVisuals();
    }

    public IEnumerator OnDamageStatusTick()
    {
        foreach (var status in statusEffects)
        {
            if (status == null) continue;

            if (status is IOnDamageTakenStatusTick)
            {
                status.Tick();
            }
        }

        statusEffects.RemoveAll(s => s.Stacks <= 0);
        UpdateStatusIcon();
        yield break;
    }


    public void Heal(int amount)
    {
        if (IsDead) return;

        CurrHP = Mathf.Min(CurrHP + amount, MaxHP);
        UpdateVisuals();

        if (G.textPopup != null)
            G.textPopup.SpawnAbove(transform, popupOffsetY, amount, isHeal: true);
    }

    public void AddShield(int amount)
    {
        if (IsDead) return;
        CurrShield += amount;
        G.audioSystem.Play(SoundId.SFX_ShieldApplied);
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
            StopAllCoroutines();
            Destroy(gameObject);
        }
    }

    public void Kill()
    {
        StartCoroutine(TakeDamage(CurrHP + CurrShield));
    }

    public void SetPos(int index)
    {
        CurrPos = index;
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

    public void SetTurnIndex(int index)
    {
        TurnOrder = index;
        UpdateTurnIndex();
    }

    public void UpdateTurnIndex()
    {
        turnIndexText.SetText("");
        if (TurnOrder > 0)
            turnIndexText.SetText(TurnOrder.ToString());
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        G.main.TryChooseTarget(this);
    }


    public IEnumerator EndTurnStatusTick()
    {
        var effects = new List<IStatusEffectInteraction>(statusEffects);
        foreach (var status in effects)
        {
            if (status == null) continue;

            if(!statusEffects.Contains(status)) continue;
            if (status is IOnTurnEndStatusInteraction endStatus)
            {
                yield return endStatus.OnTurnEndStatusEffect(this);
                yield return new WaitForSeconds(0.2f);
                if (IsDead) yield break;
            }

            if (status is IOnTurnEndStatusTick)
            {
                status.Tick();
            }
        }

        statusEffects.RemoveAll(s => s.Stacks <= 0);
        UpdateStatusIcon();
    }

    public IEnumerator OnTurn()
    {
        CurrTurnIndex++;
        currentTarget = null;

        yield return EndTurnStatusTick();

        if (IsDead || !gameObject) yield break;

        var startPos = visualRoot.position;
        var endPos = startPos;
        endPos.x -= 3f;

        var action = GetAction(CurrTurnIndex, model.EndTurnActions);
        if (action != null)
        {
            var inters = action.OnEndTurnInteractions;
            foreach (var inter in inters)
            {
                yield return inter.OnEndTurn(this);

                if (inter is not INoAnimationAction)
                {
                    visualRoot.DOMove(endPos, 0.2f);
                    yield return new WaitForSeconds(0.2f);
                    visualRoot.DOMove(startPos, 0.2f);
                    yield return new WaitForSeconds(0.2f);
                }
            }
        }

        UpdateNextActionIcon();
    }

    public ActionDef GetAction(int turnIndex, List<ActionDef> actions)
    {
        var activeAct = actions.Where(r => r != null && r.OnEndTurnInteractions.Any()).ToList();

        if (activeAct.Count == 0)
        {
            Debug.LogWarning($"Enemy {model.name} has no actions defined.");
            return null;
        }

        int index = turnIndex % activeAct.Count;
        return actions[index];
    }

    private void OnDestroy()
    {
        transform.DOKill();
        if (visualRoot != null)
            visualRoot.DOKill();
    }
}

public interface ITurnEntity
{
    public int TurnOrder { get; }
    public int Speed { get; }
    public void SetTurnIndex(int turnIndex);
    public IEnumerator OnTurn();
}