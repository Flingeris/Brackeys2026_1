using DG.Tweening;
using System;
using TMPro;
using UnityEngine;

public class TextPopupInstance : MonoBehaviour
{
    public TMP_Text label;

    [Header("Motion")]
    public float riseDistance = 1f;

    public float duration = 0.6f;

    public void Play(int amount, bool isHeal = false)
    {
        if (!label) label = GetComponentInChildren<TMP_Text>(true);
        amount = Math.Abs(amount);
        label.text = (isHeal ? "+" : "-") + amount;
        label.color = isHeal ? new Color(0.4f, 1f, 0.4f) : new Color(1f, 0.4f, 0.4f);
        label.alpha = 1f;

        var t = transform;
        t.localScale = Vector3.one * 0.9f;

        Sequence s = DOTween.Sequence();
        s.Append(t.DOMoveY(t.position.y + riseDistance, duration).SetEase(Ease.OutQuad));
        s.Join(t.DOScale(1.1f, duration));
        s.Join(label.DOFade(0f, duration).SetEase(Ease.InQuad).SetDelay(duration * 0.3f));
        s.OnComplete(() => Destroy(gameObject));
    }
}