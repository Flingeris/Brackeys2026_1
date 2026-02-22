using UnityEngine;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class CardGallery : MonoBehaviour
{
    [Min(1)] public int columns = 5;
    public float spacingX = 2f;
    public float spacingY = 3f;

    [Tooltip("Если true – галерея будет перестраиваться при изменении параметров в инспекторе")]
    public bool autoRebuild = false;

    [ContextMenu("Rebuild Card Gallery")]
    public void Rebuild()
    {
        #if UNITY_EDITOR
        // 1) Чистим старых детей
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            var child = transform.GetChild(i);

            if (Application.isPlaying)
                Destroy(child.gameObject);
            else
                DestroyImmediate(child.gameObject);
        }

        // 2) Берём все CardModel из CMS
        var allCards = CMS.GetAll<CardModel>().ToList();
        if (allCards.Count == 0)
        {
            Debug.LogWarning("[CardGallery] CMS.GetAll<CardModel>() вернуло 0 карт");
            return;
        }

        // 3) Спавним и раскладываем
        int index = 0;

        foreach (var model in allCards)
        {
            if (model == null || model.Prefab == null)
                continue;

            // В редакторе удобно спавнить как prefab instance
            CardInstance inst =
                PrefabUtility.InstantiatePrefab(model.Prefab, transform) as CardInstance;

            if (inst == null)
                continue;

            inst.SetModel(model);

            int col = index % columns;
            int row = index / columns;

            inst.transform.localPosition = new Vector3(
                col * spacingX,
                -row * spacingY,
                0f
            );

            index++;
        }

        Debug.Log($"[CardGallery] Построено {index} карточек");
        #else
        Debug.LogWarning("[CardGallery] Rebuild() работает только в редакторе (UNITY_EDITOR).");
        #endif
    }

    private void OnValidate()
    {
        #if UNITY_EDITOR
        if (!Application.isPlaying && autoRebuild)
        {
            Rebuild();
        }
        #endif
    }
}