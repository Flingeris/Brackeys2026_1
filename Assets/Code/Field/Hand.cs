using System.Collections;
using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Hand : MonoBehaviour, IDraggableOwner<DraggableCard>
{
    public List<CardInstance> cardsInHand = new();
    [SerializeField] private Transform cardsParent;

    private const float PLACEMENT_X_RANGE = 1.75f;

    [Header("Hover layout")] [SerializeField]
    private float hoveredBottomY = -3f; // локальный Y для наведённой карты

    [SerializeField] private float hoverSpread = 0.6f;
    [SerializeField] private float hoverSpreadFar = 0.3f;
    [SerializeField] private float layoutLerpSpeed = 12f;


    private CardInstance _hoveredCard;

    public void SetHovered(CardInstance card)
    {
        if (card == null) return;
        _hoveredCard = card;
    }

    public void ClearHovered(CardInstance card)
    {
        if (_hoveredCard == card)
            _hoveredCard = null;
    }

    private void Awake()
    {
        G.Hand = this;
    }

    private void Update()
    {
        if (cardsInHand.Count > 0) UpdateCardsPositions();
    }

    private void GetBaseLayout(int index, out Vector3 pos, out Quaternion rot)
    {
        float t = (cardsInHand.Count == 1)
            ? 0.5f
            : (float)index / (cardsInHand.Count - 1);

        float xPos = Mathf.Lerp(-6f / 2f, 6f / 2f, t);
        float centerT = t - 0.5f;
        float yPos = 0.5f * (1f - Mathf.Abs(centerT) * 2f);
        float rotationZ = -15f * centerT * 2f;

        pos = new Vector3(xPos, yPos, 0f);
        rot = Quaternion.Euler(0f, 0f, rotationZ);
    }

    private void ApplyHoverOffset(int index, int hoveredIndex, ref Vector3 pos, ref Quaternion rot)
    {
        if (hoveredIndex == -1)
            return;

        int delta = index - hoveredIndex;

        float extraX = 0f;

        if (delta == -1) extraX -= hoverSpread;
        else if (delta == 1) extraX += hoverSpread;
        else if (delta <= -2) extraX -= hoverSpreadFar;
        else if (delta >= 2) extraX += hoverSpreadFar;

        pos.x += extraX;

        if (index == hoveredIndex)
        {
            rot = Quaternion.identity;

            pos.y = hoveredBottomY;
        }
    }

    private void UpdateCardsPositions()
    {
        if (cardsInHand.Count == 0)
            return;

        int hoveredIndex = _hoveredCard != null
            ? cardsInHand.IndexOf(_hoveredCard)
            : -1;

        for (int i = 0; i < cardsInHand.Count; i++)
        {
            var card = cardsInHand[i];
            if (card == null)
                continue;

            var dr = card.Draggable;

            // важно: НЕ пропускать IsReturning, иначе карта может навсегда остаться вне layout’а
            if (dr != null && dr.IsDragging)
                continue;

            GetBaseLayout(i, out var localPos, out var localRot);
            ApplyHoverOffset(i, hoveredIndex, ref localPos, ref localRot);

            var targetWorldPos = cardsParent.TransformPoint(localPos);
            var targetWorldRot = cardsParent.rotation * localRot;

            var t = card.transform;
            float s = layoutLerpSpeed * Time.deltaTime;
            t.position = Vector3.Lerp(t.position, targetWorldPos, s);
            t.rotation = Quaternion.Slerp(t.rotation, targetWorldRot, s);
        }
    }

    public void Claim(CardInstance card)
    {
        card.Draggable.SetOwner(this);
    }


    public void AddCard(CardState state)
    {
        var inst = CreateCard(state.model.Id);
        inst.state = state;
        Claim(inst);
    }

    public CardInstance CreateCard(string id)
    {
        var cardModel = CMS.Get<CardModel>(id);
        var cardInst = Instantiate(cardModel.Prefab, cardsParent, false);
        cardInst.SetModel(cardModel);
        cardInst.Draggable.SetOwner(this);
        return cardInst;
    }


    public IEnumerator Clear()
    {
        // for (int i = 0; i < cardsInHand.Count; i++)
        // {
        //     var d = cardsInHand[i];
        //     if (d == null) continue;
        //
        //     Destroy(d.gameObject);
        //     cardsInHand[i] = null;
        // }
        var cards = new List<CardInstance>(cardsInHand);

        foreach (var cardInstance in cards)
        {
            if (cardInstance == null) continue;
            yield return G.main.KillCard(cardInstance);
        }

        cardsInHand.Clear();
    }

    public void DrawControlledHand(int handSize)
    {
        // Минимальные количества (можешь потом подправить)
        const int MIN_DAMAGE = 2;
        const int MIN_TANK = 1;
        const int MIN_HEAL = 1;

        // Берём все модели карт из CMS
        var allCards = CMS.GetAll<CardModel>().ToList();

        // Разбиваем по классам
        var damageCards = allCards
            .Where(c => c.ClassType == ClassType.Damage)
            .ToList();

        var tankCards = allCards
            .Where(c => c.ClassType == ClassType.Tank)
            .ToList();

        var healCards = allCards
            .Where(c => c.ClassType == ClassType.Heal)
            .ToList();

        // Сюда будем складывать выбранные модели
        var modelsToSpawn = new List<CardModel>();
        int remaining = handSize;

        // Локальная функция спавна по модели
        void SpawnCard(CardModel model)
        {
            if (model == null) return;

            var cardInst = Instantiate(model.Prefab, cardsParent, false);
            cardInst.SetModel(model);
            cardInst.Draggable.SetOwner(this);
        }

        // Локальная функция — добавить N рандомных карт из списка (с дубликатами)
        void AddRandomFrom(List<CardModel> source, int count)
        {
            if (source == null || source.Count == 0) return;

            for (int i = 0; i < count; i++)
            {
                var model = source.GetRandomElement(); // твой GameUtils.GetRandomElement
                modelsToSpawn.Add(model);
            }
        }

        // 1) Гарантированные уроновые
        if (damageCards.Count > 0 && remaining > 0)
        {
            int count = Mathf.Min(MIN_DAMAGE, remaining);
            AddRandomFrom(damageCards, count);
            remaining -= count;
        }

        // 2) Гарантированные щиты
        if (tankCards.Count > 0 && remaining > 0)
        {
            int count = Mathf.Min(MIN_TANK, remaining);
            AddRandomFrom(tankCards, count);
            remaining -= count;
        }

        // 3) Гарантированные хильные
        if (healCards.Count > 0 && remaining > 0)
        {
            int count = Mathf.Min(MIN_HEAL, remaining);
            AddRandomFrom(healCards, count);
            remaining -= count;
        }

        // 4) Остаток — чем угодно (как твой обычный Draw)
        while (remaining > 0 && allCards.Count > 0)
        {
            var model = allCards.GetRandomElement();
            modelsToSpawn.Add(model);
            remaining--;
        }

        // 5) Спавним всё, что выбрали
        foreach (var model in modelsToSpawn)
        {
            SpawnCard(model);
        }
    }

    public void OnDragEnter(DraggableCard d)
    {
        var card = d.instance;
        if (!cardsInHand.Contains(card))
            cardsInHand.Add(card);

        var t = card.transform;

        // 1) Гасим всё, что могло двигать root
        t.DOKill();
        d.transform.DOKill();

        // 2) Сажаем строго под hand-parent (без сохранения world)
        t.SetParent(cardsParent, false);

        // 3) Обнуляем локальные координаты, дальше рука сама разложит
        t.localPosition = Vector3.zero;
        t.localRotation = Quaternion.identity;

        // --- RESET VISUAL HOVER ---
        var hover = card.GetComponentInChildren<CardHoverVFX>();
        if (hover != null)
        {
            hover.ResetVisual(); // мы сейчас напишем метод
        }
    }


    public void OnDragExit(DraggableCard d)
    {
        var card = d.instance;
        cardsInHand.Remove(card);
        // StartCoroutine(DragExitSequence(d));
    }

    public IEnumerator DragExitSequence(DraggableCard d)
    {
        var card = d.instance;
        cardsInHand.Remove(card);
        yield break;
        // yield return G.main.OnCardPlayed(card);
    }
}