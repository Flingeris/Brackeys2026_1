#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

/// <summary>
/// Простое окно-эксплорер для ContentDef:
/// - находит все ассеты ContentDef;
/// - группирует по типу (ItemDef, DiceDef и т.п.);
/// - поиск по имени / Id / типу;
/// - двойной клик пингует ассет в Project.
/// </summary>
public class ContentDefExplorerWindow : EditorWindow
{
    [Serializable]
    private class ContentDefInfo
    {
        public string Guid;
        public string AssetPath;
        public string FolderPath;      // относительный путь папки (Assets/.../...)
        public string Name;            // имя ассета
        public string Id;              // ContentDef.Id
        public string TypeName;        // короткое имя типа (ItemDef, DiceDef...)
        public ContentDef Asset;       // ссылка на сам ассет
    }

    private class ContentDefTreeItem : TreeViewItem
    {
        public ContentDefInfo Info;

        public ContentDefTreeItem(int id, int depth, string displayName, ContentDefInfo info)
            : base(id, depth, displayName)
        {
            Info = info;
        }
    }

    private class ContentDefTreeView : TreeView
    {
        private readonly List<ContentDefInfo> _allItems = new();
        private readonly Dictionary<int, ContentDefInfo> _idToInfo = new();
        private Action<ContentDefInfo> _onItemDoubleClick;

        public ContentDefTreeView(TreeViewState state) : base(state)
        {
            showBorder = true;
            rowHeight = EditorGUIUtility.singleLineHeight + 2;
        }

        public void SetData(IEnumerable<ContentDefInfo> items, Action<ContentDefInfo> onItemDoubleClick)
        {
            _allItems.Clear();
            _allItems.AddRange(items);
            _onItemDoubleClick = onItemDoubleClick;
            Reload();
        }

        protected override TreeViewItem BuildRoot()
        {
            // Корневой элемент (обязателен, id = 0, depth = -1)
            var root = new TreeViewItem
            {
                id = 0,
                depth = -1,
                displayName = "Root"
            };

            _idToInfo.Clear();

            if (_allItems.Count == 0)
            {
                // Пустой список
                root.AddChild(new TreeViewItem(1, 0, "No ContentDef assets found"));
                return root;
            }

            // Группируем по типу ContentDef (ItemDef, DiceDef и т.п.)
            var groups = _allItems
                .GroupBy(i => i.TypeName)
                .OrderBy(g => g.Key, StringComparer.OrdinalIgnoreCase);

            int idCounter = 1;

            foreach (var group in groups)
            {
                var groupItem = new TreeViewItem(idCounter++, 0, group.Key);
                root.AddChild(groupItem);

                // Внутри группы сортируем по имени
                foreach (var info in group.OrderBy(i => i.Name, StringComparer.OrdinalIgnoreCase))
                {
                    var child = new ContentDefTreeItem(idCounter++, 1, info.Name, info);
                    groupItem.AddChild(child);
                    _idToInfo[child.id] = info;
                }
            }

            // Обязательный вызов: сетим root и возвращаем
            SetupDepthsFromParentsAndChildren(root);
            return root;
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            base.RowGUI(args);

            // Для листовых элементов (наши ContentDef) рисуем доп.инфу справа
            if (args.item is ContentDefTreeItem item && item.Info != null)
            {
                var rect = args.rowRect;
                rect.x += GetContentIndent(args.item) + 16f; // небольшой отступ от иконки/текста
                rect.width -= GetContentIndent(args.item) + 16f;

                // Лейаут: [Имя] | [Id] | [Папка]
                // Имя уже рисует базовый RowGUI, мы дорисуем Id и папку справа мелким текстом
                using (new EditorGUI.DisabledScope(true))
                {
                    var label = $"Id: {item.Info.Id}   •   Folder: {item.Info.FolderPath}";
                    var style = EditorStyles.miniLabel;
                    var size = style.CalcSize(new GUIContent(label));

                    var rightRect = new Rect(
                        rect.xMax - size.x - 8f,
                        rect.y + (rect.height - size.y) * 0.5f,
                        size.x + 8f,
                        size.y);

                    GUI.Label(rightRect, label, style);
                }
            }
        }

        protected override void DoubleClickedItem(int id)
        {
            if (_idToInfo.TryGetValue(id, out var info) && info.Asset != null)
            {
                _onItemDoubleClick?.Invoke(info);
            }
        }
    }

    // --- поля окна ---

    private TreeViewState _treeViewState;
    private ContentDefTreeView _treeView;
    private SearchField _searchField;
    private string _searchText = string.Empty;
    private List<ContentDefInfo> _allDefs = new();
    private Vector2 _statusScroll;

    // --- меню ---

    [MenuItem("Tools/Content/ContentDef Explorer")]
    public static void Open()
    {
        var window = GetWindow<ContentDefExplorerWindow>("ContentDef Explorer");
        window.Show();
    }

    private void OnEnable()
    {
        if (_treeViewState == null)
            _treeViewState = new TreeViewState();

        _treeView ??= new ContentDefTreeView(_treeViewState);
        _searchField ??= new SearchField();

        RefreshData();
    }

    private void OnGUI()
    {
        DrawToolbar();

        var rect = GUILayoutUtility.GetRect(0, 100000, 0, 100000);
        _treeView.OnGUI(rect);

        DrawStatusBar();
    }

    private void DrawToolbar()
    {
        using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
        {
            // Кнопка обновления
            if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.Width(70)))
            {
                RefreshData();
            }

            GUILayout.Space(8);

            // Поиск
            GUILayout.Label("Search:", EditorStyles.toolbarButton, GUILayout.Width(50));
            var newSearch = _searchField.OnToolbarGUI(_searchText);
            if (newSearch != _searchText)
            {
                _searchText = newSearch;
                ApplyFilter();
            }

            GUILayout.FlexibleSpace();

            // Инфа по количеству
            GUILayout.Label($"Total: {_allDefs.Count}", EditorStyles.miniLabel);
        }
    }

    private void DrawStatusBar()
    {
        using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
        {
            _statusScroll = EditorGUILayout.BeginScrollView(_statusScroll, GUILayout.Height(EditorGUIUtility.singleLineHeight * 2));

            if (_allDefs.Count == 0)
            {
                EditorGUILayout.LabelField("No ContentDef assets found in the project.", EditorStyles.miniLabel);
            }
            else
            {
                EditorGUILayout.LabelField("Hint: double-click a definition to ping it in the Project window.", EditorStyles.miniLabel);
                EditorGUILayout.LabelField("Search works by Name, Id and Type name.", EditorStyles.miniLabel);
            }

            EditorGUILayout.EndScrollView();
        }
    }

    // --- логика загрузки и фильтрации ---

    private void RefreshData()
    {
        _allDefs.Clear();

        // Ищем ВСЕ ContentDef в проекте
        var guids = AssetDatabase.FindAssets("t:ContentDef");

        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var asset = AssetDatabase.LoadAssetAtPath<ContentDef>(path);
            if (asset == null)
                continue;

            var folder = Path.GetDirectoryName(path)?.Replace("\\", "/") ?? string.Empty;
            var type = asset.GetType();
            var info = new ContentDefInfo
            {
                Guid = guid,
                AssetPath = path,
                FolderPath = folder,
                Name = asset.name,
                Id = asset.Id,
                TypeName = type.Name,
                Asset = asset
            };

            _allDefs.Add(info);
        }

        ApplyFilter();
    }

    private void ApplyFilter()
    {
        IEnumerable<ContentDefInfo> result = _allDefs;

        if (!string.IsNullOrWhiteSpace(_searchText))
        {
            var search = _searchText.Trim();
            result = result.Where(i =>
                i.Name.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0 ||
                i.Id.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0 ||
                i.TypeName.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0 ||
                i.FolderPath.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0
            );
        }

        _treeView.SetData(result, OnItemDoubleClicked);
    }

    private void OnItemDoubleClicked(ContentDefInfo info)
    {
        if (info.Asset == null)
            return;

        Selection.activeObject = info.Asset;
        EditorGUIUtility.PingObject(info.Asset);
    }
}
#endif
