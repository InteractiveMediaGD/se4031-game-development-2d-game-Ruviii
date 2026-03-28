#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Editor utilities for scene health (missing scripts often correlate with Inspector exceptions).
/// </summary>
public static class WingsOfAshSceneTools
{
    [MenuItem("Wings of Ash/Clear Selection (Inspector recovery)")]
    public static void ClearSelection()
    {
        Selection.activeObject = null;
        Debug.Log("[Wings of Ash] Selection cleared. If GameObjectInspector errors continue, try Window → Layouts → Default and restart the Editor.");
    }

    [MenuItem("Wings of Ash/Report Missing Scripts In Loaded Scenes")]
    public static void ReportMissingScriptsInLoadedScenes()
    {
        int totalMissing = 0;
        for (int s = 0; s < SceneManager.sceneCount; s++)
        {
            Scene scene = SceneManager.GetSceneAt(s);
            if (!scene.isLoaded || !scene.IsValid())
            {
                continue;
            }

            GameObject[] roots = scene.GetRootGameObjects();
            for (int r = 0; r < roots.Length; r++)
            {
                totalMissing += ReportMissingRecursive(roots[r].transform, scene.name);
            }
        }

        if (totalMissing == 0)
        {
            Debug.Log("[Wings of Ash] No missing script slots found in loaded scenes.");
        }
        else
        {
            Debug.LogWarning($"[Wings of Ash] Found {totalMissing} missing script slot(s). Remove them (Inspector → ⋮ → Remove Component) or reassign scripts.");
        }
    }

    [MenuItem("Wings of Ash/Report Missing Scripts In WingsOfAsh Prefabs")]
    public static void ReportMissingScriptsInProjectPrefabs()
    {
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/WingsOfAsh" });
        int totalMissing = 0;
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                totalMissing += ReportMissingRecursive(root.transform, path);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        if (totalMissing == 0)
        {
            Debug.Log("[Wings of Ash] No missing script slots found on prefabs under Assets/WingsOfAsh.");
        }
        else
        {
            Debug.LogWarning(
                $"[Wings of Ash] Found {totalMissing} missing script slot(s) on prefabs. Open each prefab, remove or fix the Missing (Mono Script) component.");
        }
    }

    private static int ReportMissingRecursive(Transform t, string sceneName)
    {
        int count = 0;
        GameObject go = t.gameObject;
        int missing = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(go);
        if (missing > 0)
        {
            count += missing;
            string path = GetPath(t);
            Debug.LogWarning($"[Wings of Ash] Missing script on '{path}' in scene '{sceneName}' ({missing} slot(s)).", go);
        }

        for (int i = 0; i < t.childCount; i++)
        {
            count += ReportMissingRecursive(t.GetChild(i), sceneName);
        }

        return count;
    }

    private static string GetPath(Transform t)
    {
        if (t.parent == null)
        {
            return t.name;
        }

        return GetPath(t.parent) + "/" + t.name;
    }
}
#endif
