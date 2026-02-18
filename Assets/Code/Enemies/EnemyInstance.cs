using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class EnemyInstance : MonoBehaviour, ITurnEntity, ICombatEntity, IPointerClickHandler
{
    public EnemyModel model;
    public int MaxHP { get; private set; }
    public int Speed { get; private set; }
    public int CurrHP { get; private set; }
    public bool IsDead { get; private set; }
    public int CurrShield { get; private set; }
    public bool IsPossibleTarget { get; private set; }
    public int TurnOrder { get; private set; }
    public int CurrTurnIndex = -1;


    public CombatGroup combatGroup;


    [SerializeField] private TMP_Text hpValueText;
    [SerializeField] private SpriteRenderer highlight;
    [SerializeField] private SpriteRenderer sprite;
    [SerializeField] private TMP_Text turnIndexText;


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
            Destroy(gameObject);
        }
    }

    public void Kill()
    {
        TakeDamage(CurrHP);
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

    public IEnumerator OnTurnEnd()
    {
        CurrTurnIndex++;

        var startPos = transform.position;
        var endPos = startPos;
        endPos.x -= 3f;
        transform.DOMove(endPos, 0.2f);
        yield return new WaitForSeconds(0.2f);

        var action = GetNextAction(CurrTurnIndex, model.EndTurnActions);
        
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
    }

    public ActionDef GetNextAction(int turnIndex, List<ActionDef> actions)
    {
        var activeAct = actions.Where(r => r != null && r.OnEndTurnInteractions.Any()).ToList();

        if (activeAct.Count == 0)
        {
            Debug.LogError($"Enemy {model.name} has no actions defined.");
            return null;
        }

        int index = turnIndex % actions.Count;
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