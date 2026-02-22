using DG.Tweening;
using System;
using TMPro;
using UnityEngine;

public class TextPopupInstance : MonoBehaviour
{
    public TMP_Text label;
    private Sequence s;

    [Header("Motion")] public float riseDistance = 1f;
    public float duration = 0.6f;

    public void Play(int amount, bool isHeal = false, bool isMaxHpLoss = false)
    {
        if (!label) label = GetComponentInChildren<TMP_Text>(true);

        amount = Math.Abs(amount);

        // Текст
        string sign = isHeal ? "+" : "-";
        string suffix = isMaxHpLoss ? " Max HP" : string.Empty;
        label.text = sign + amount + suffix;

        // Цвет
        if (isMaxHpLoss)
        {
            label.color = new Color(1f, 0.8f, 0.3f);
        }
        else
        {
            label.color = isHeal
                ? new Color(0.4f, 1f, 0.4f)
                : new Color(1f, 0.4f, 0.4f);
        }

        label.alpha = 1f;

        var t = transform;
        t.localScale = Vector3.one * 0.9f;

        s = DOTween.Sequence();
        s.Append(t.DOMoveY(t.position.y + riseDistance, duration).SetEase(Ease.OutQuad));
        s.Join(t.DOScale(1.1f, duration));
        s.Join(label.DOFade(0f, duration).SetEase(Ease.InQuad).SetDelay(duration * 0.3f));
        s.OnComplete(() => Destroy(gameObject));
    }

    private void OnDestroy()
    {
        if (s != null && s.IsActive())
            s.Kill();

        transform.DOKill();
    }
}