using UnityEngine;
using DG.Tweening;

public class BreathingAnimation : MonoBehaviour
{
    [Header("Scale (масштаб)")]
    public float scaleAmplitude = 0.05f;
    public float scaleDuration  = 0.8f;

    [Header("MoveY (подпрыгивание)")]
    public float moveYAmplitude = 0.0f;
    public float moveYDuration  = 0.8f;

    [Header("Прочее")]
    public bool playOnStart = true;

    [Tooltip("Максимальная рандомная задержка старта, чтобы дыхание не было синхронным.")]
    public float maxRandomStartDelay = 0.5f;

    private Sequence _breathSeq;
    private Vector3 _startScale;
    private Vector3 _startPos;

    void Awake()
    {
        _startScale = transform.localScale;
        _startPos   = transform.localPosition;
    }

    void Start()
    {
        if (playOnStart)
            StartBreathing();
    }

    public void StartBreathing()
    {
        if (_breathSeq != null && _breathSeq.IsActive())
            _breathSeq.Kill();

        transform.localScale    = _startScale;
        transform.localPosition = _startPos;

        _breathSeq = DOTween.Sequence();

        _breathSeq.Append(
            transform
                .DOScale(_startScale * (1f + scaleAmplitude), scaleDuration)
                .SetEase(Ease.InOutSine)
        );

        if (Mathf.Abs(moveYAmplitude) > 0.0001f)
        {
            _breathSeq.Join(
                transform
                    .DOLocalMoveY(_startPos.y + moveYAmplitude, moveYDuration)
                    .SetEase(Ease.InOutSine)
            );
        }

        _breathSeq
            .SetLoops(-1, LoopType.Yoyo)
            .SetUpdate(true);

        float delay = Random.Range(0f, maxRandomStartDelay);
        _breathSeq.SetDelay(delay);
    }

    public void StopBreathing()
    {
        if (_breathSeq != null && _breathSeq.IsActive())
            _breathSeq.Kill();

        transform.localScale    = _startScale;
        transform.localPosition = _startPos;
    }

    void OnDisable()
    {
        if (_breathSeq != null && _breathSeq.IsActive())
            _breathSeq.Kill();
    }

    void OnDestroy()
    {
        if (_breathSeq != null && _breathSeq.IsActive())
            _breathSeq.Kill();
    }
}