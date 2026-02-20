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

public class EnemyInstance : MonoBehaviour, ITurnEntity, ICombatEntity, IPointerClickHandler
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


    public List<IStatusEffectInteraction> statusEffects { get; set; } = new();

    public ActionDef nextAction;
    public CombatGroup combatGroup;

    [Header("Visual References")] [SerializeField]
    private TMP_Text hpValueText;

    [SerializeField] private SpriteRenderer highlight;
    [SerializeField] private SpriteRenderer sprite;
    [SerializeField] private TMP_Text turnIndexText;
    [SerializeField] private SpriteRenderer actionIconImage;
    [SerializeField] private TMP_Text actionValueText;

    [SerializeField] private SpriteRenderer statusEffectsIcons;
    [SerializeField] private TMP_Text statusEffectsText;
    
    [Header("Popup")]
    [SerializeField] private float popupOffsetY = 1.5f;


    private void Start()
    {
        UpdateVisuals();
    }

    public void SetTarget(bool b)
    {
        IsPossibleTarget = b;
    }

    public void SetModel(EnemyModel newModel)
    {
        MaxHP = newModel.StartingHealth;
        model = newModel;
        if (model == null) return;
        Speed = model.Speed;
        CurrHP = model.StartingHealth;
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        UpdateHpText();
        UpdateSprite();
        UpdateNextActionIcon();
        UpdateStatusIcon();
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

        if (nextAction.Amount != 0)
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

    private void UpdateSprite()
    {
        if (sprite == null) return;
        sprite.sprite = model.Sprite;
    }

    private void UpdateHpText()
    {
        hpValueText.SetText("");
        if (model == null) return;

        hpValueText.SetText(CurrHP.ToString() + " / " + model.StartingHealth.ToString());
    }


    public void TakeDamage(int dmgAmount)
    {
        if (IsDead) return;
        if (dmgAmount <= 0) return;

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
        CheckIsDead();
        UpdateVisuals();
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
        UpdateVisuals();
    }

    public void SetShield(int amount)
    {
        if (IsDead) return;
        CurrShield = 0;
        AddShield(amount);
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
        TakeDamage(CurrHP);
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
        Debug.Log("On enemy clicked");
        G.main.TryChooseTarget(this);
    }

    public IEnumerator OnTurnEnd()
    {
        CurrTurnIndex++;


        foreach (var status in statusEffects)
        {
            if (status is IOnTurnEndStatusInteraction endStatus)
            {
                yield return endStatus.OnTurnEndTick(this);
                yield return new WaitForSeconds(0.2f);
                if (IsDead) yield break;
            }

            status.Tick();
        }

        statusEffects.RemoveAll(s => s.Stacks <= 0);
        UpdateStatusIcon();

        if (IsDead || !gameObject) yield break;

        var startPos = transform.position;
        var endPos = startPos;
        endPos.x -= 3f;
        transform.DOMove(endPos, 0.2f);
        yield return new WaitForSeconds(0.2f);

        var action = GetAction(CurrTurnIndex, model.EndTurnActions);

        if (action != null)
        {
            var inters = action.OnEndTurnInteractions;
            foreach (var inter in inters)
            {
                yield return inter.OnEndTurn(this);
            }
        }

        transform.DOMove(startPos, 0.2f);
        yield return new WaitForSeconds(0.2f);

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
}

public interface ITurnEntity
{
    public int TurnOrder { get; }
    public int Speed { get; }
    public void SetTurnIndex(int turnIndex);
    public IEnumerator OnTurnEnd();
}