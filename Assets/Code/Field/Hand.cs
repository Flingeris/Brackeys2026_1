using System.Collections;
using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public class Hand : MonoBehaviour, IDraggableOwner<DraggableCard>
{
    public List<CardInstance> cardsInHand = new();
    [SerializeField] private Transform cardsParent;

    private const float PLACEMENT_X_RANGE = 1.75f;
    
    [Header("Hover layout")]
    [SerializeField] private float hoveredBottomY = -3f; // локальный Y для наведённой карты
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

        if (delta == -1)       extraX -= hoverSpread;
        else if (delta == 1)   extraX += hoverSpread;
        else if (delta <= -2)  extraX -= hoverSpreadFar;
        else if (delta >= 2)   extraX += hoverSpreadFar;

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



    
    public void Draw()
    {
        var allCards = CMS.GetAll<CardModel>();
        var cardModel = allCards.GetRandomElement();
        var cardInst = Instantiate(cardModel.Prefab, cardsParent, false);
        cardInst.SetModel(cardModel);
        cardInst.Draggable.SetOwner(this);
    }
    
    public void Clear()
    {
        for (int i = 0; i < cardsInHand.Count; i++)
        {
            var d = cardsInHand[i];
            if (d == null) continue;
            
            Destroy(d.gameObject);
            cardsInHand[i] = null;
        }

        cardsInHand.Clear();
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
        if (d == null || d.instance == null) return;

        cardsInHand.Remove(d.instance);
    }

    public IEnumerator DragExitSequence(DraggableCard d)
    {
        var card = d.instance;
        cardsInHand.Remove(card);
        yield return G.main.OnCardPlayed(card);
    }
}