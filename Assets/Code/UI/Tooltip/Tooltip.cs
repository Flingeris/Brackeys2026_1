using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Tooltip : MonoBehaviour
{
    [Header("Tooltip settings")] [SerializeField]
    private TMP_Text nameText;

    [SerializeField] private TMP_Text descText;
    private Camera _camera;
    [SerializeField] private RectTransform Rect;
    [SerializeField] private Canvas canvas;
    [SerializeField] private LayoutElement layout;
    [SerializeField] private int minWidth = 70;
    public Vector2 offset;
    [SerializeField] private Vector2 offscreenOffset;

    [Header("Layout")]
    [SerializeField] private VerticalLayoutGroup verticalLayout;
    [SerializeField] private int topPaddingWithTitle = 38;
    [SerializeField] private int topPaddingWithoutTitle = 20;
    [SerializeField] private int spacingWithTitle = 12;
    [SerializeField] private int spacingWithoutTitle = 6;
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

    public void Show(ITooltipInfo data, GameObject user)
    {
        if (IsBlocked) return;
        if (data == null) return;

        currUser = user;
        if (string.IsNullOrEmpty(data.Description) && string.IsNullOrEmpty(data.ItemName))
            return;

        bool hasTitle = !string.IsNullOrEmpty(data.ItemName);

        nameText.text = data.ItemName;
        descText.text = data.Description;

        if (nameText != null)
            nameText.gameObject.SetActive(hasTitle);

        if (verticalLayout != null)
        {
            var padding = verticalLayout.padding;
            padding.top = hasTitle ? topPaddingWithTitle : topPaddingWithoutTitle;
            verticalLayout.padding = padding;

            verticalLayout.spacing = hasTitle ? spacingWithTitle : spacingWithoutTitle;
        }
        

        var textWidth = nameText.preferredWidth + 6;
        layout.preferredWidth = textWidth > minWidth ? textWidth : minWidth;

        LayoutRebuilder.ForceRebuildLayoutImmediate(Rect); 
        // Rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, layout.preferredHeight);

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

        ClampToScreen();
    }

    private void ClampToScreen()
    {
        if (canvas == null || Rect == null)
            return;

        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        if (canvasRect == null)
            return;

        Vector2 size = Rect.rect.size;
        Vector2 pos = Rect.anchoredPosition;
        Vector2 pivot = Rect.pivot;

        Rect cRect = canvasRect.rect;

        float minX = cRect.xMin + offscreenOffset.x + pivot.x * size.x;
        float maxX = cRect.xMax - offscreenOffset.x - (1f - pivot.x) * size.x;

        float minY = cRect.yMin + offscreenOffset.y + pivot.y * size.y;
        float maxY = cRect.yMax - offscreenOffset.y - (1f - pivot.y) * size.y;

        if (minX > maxX) (minX, maxX) = (maxX, minX);
        if (minY > maxY) (minY, maxY) = (maxY, minY);

        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);

        Rect.anchoredPosition = pos;
    }
}