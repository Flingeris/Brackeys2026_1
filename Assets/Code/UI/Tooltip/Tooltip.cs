using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Tooltip : MonoBehaviour
{
    [Header("Tooltip settings")] [SerializeField]
    private TMP_Text nameText;

    private Camera _camera;
    [SerializeField] private TMP_Text descText;
    [SerializeField] private RectTransform Rect;
    [SerializeField] private Canvas canvas;
    [SerializeField] private LayoutElement layout;
    [SerializeField] private int minWidth = 70;
    public Vector2 offset;
    [SerializeField] private Vector2 offscreenOffset = new Vector2(10f, 10f);

    public bool IsBlocked => blockersCount > 0;
    private int blockersCount = 0;
    private GameObject currUser;

    public void PushBlock()
    {
        blockersCount++;
        Hide();
    }

    public void PopBlock()
    {
        blockersCount = Mathf.Max(0, blockersCount - 1);
    }

    private void Start()
    {
        Hide();
        _camera = Camera.main;
    }

    // public void Show(ITooltipInfo data, bool showAdditionalTooltip, GameObject user)
    // {
    //     Show(data, user);
    // }

    private void Show(ITooltipInfo data, GameObject user)
    {
        //Debug.Log("Showing tooltip for: " + data.ItemName);
        if (IsBlocked) return;
        if (data == null) return;
        this.currUser = user;
        if (string.IsNullOrEmpty(data.Description) && string.IsNullOrEmpty(data.ItemName)) return;

        nameText.text = data.ItemName;
        descText.text = data.Description;
        var textWidth = nameText.preferredWidth + 6;
        layout.preferredWidth = textWidth > minWidth ? textWidth : minWidth;

        UpdatePos();
        Rect.gameObject.SetActive(true);
    }

    public void Hide()
    {
        currUser = null;
        Rect.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!Rect.gameObject.activeSelf) return;
        if (!currUser || !currUser.activeInHierarchy)
        {
            Hide();
            return;
        }

        UpdatePos();
    }

    private void UpdatePos()
    {
        Vector2 mousePos = Input.mousePosition;

        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, mousePos, _camera, out var localPoint);
        localPoint += offset;
        Rect.anchoredPosition = localPoint;
    }
}