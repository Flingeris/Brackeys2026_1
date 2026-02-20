using UnityEngine;

public class HpBarView : MonoBehaviour
{
    [SerializeField] private Transform fill;

    private Vector3 _fullScale;

    private void Awake()
    {
        if (fill == null)
        {
            var t = transform.Find("Fill");
            if (t != null)
                fill = t;
        }

        if (fill != null)
            _fullScale = fill.localScale;
    }
    
    public void SetNormalized(float value)
    {
        if (fill == null) return;

        value = Mathf.Clamp01(value);
        fill.localScale = new Vector3(
            _fullScale.x * value,
            _fullScale.y,
            _fullScale.z
        );
    }
}