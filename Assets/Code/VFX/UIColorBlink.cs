using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

[RequireComponent(typeof(Image))]
public class UIColorBlinkDOTween : MonoBehaviour
{
    [Header("Colors")]
    public Color colorA = Color.red;
    public Color colorB = Color.white;

    [Header("Speed")]
    [Tooltip("Сколько раз в секунду проходит полный цикл A->B->A")]
    public float cyclesPerSecond = 2f;

    [Header("Mode")]
    [Tooltip("Если включить — будет резкое мигание (без плавности)")]
    public bool hardBlink = false;

    private Image img;
    private Tween blinkTween;

    void Awake()
    {
        img = GetComponent<Image>();
    }

    void OnEnable()
    {
        StartBlink();
    }

    void OnDisable()
    {
        KillBlink();
    }

    void OnDestroy()
    {
        KillBlink();
    }

    void OnValidate()
    {
        if (!Application.isPlaying) return;
        if (!isActiveAndEnabled) return;

        StartBlink();
    }

    private void StartBlink()
    {
        KillBlink();

        if (cyclesPerSecond <= 0f)
        {
            if (img != null)
                img.color = colorA;
            return;
        }

        img.color = colorA;

        float halfCycleDuration = 1f / (2f * cyclesPerSecond);

        if (!hardBlink)
        {
            blinkTween = img
                .DOColor(colorB, halfCycleDuration)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.Linear);
        }
        else
        {
            float halfPeriod = 0.5f / cyclesPerSecond;

            Sequence seq = DOTween.Sequence();
            seq.AppendCallback(() => img.color = colorB);
            seq.AppendInterval(halfPeriod);
            seq.AppendCallback(() => img.color = colorA);
            seq.AppendInterval(halfPeriod);

            blinkTween = seq
                .SetLoops(-1, LoopType.Restart);
        }
    }

    private void KillBlink()
    {
        if (blinkTween != null && blinkTween.IsActive())
        {
            blinkTween.Kill();
            blinkTween = null;
        }
    }
}