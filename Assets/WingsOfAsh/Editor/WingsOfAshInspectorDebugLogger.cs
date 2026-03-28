#if UNITY_EDITOR
using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

/// <summary>
/// Debug: correlates Editor Inspector exceptions with selection (session 4f0c84).
/// Off by default — set <see cref="Enabled"/> to true only while diagnosing Inspector issues.
/// </summary>
[InitializeOnLoad]
public static class WingsOfAshInspectorDebugLogger
{
    /// <summary>Enable NDJSON logging to debug-4f0c84.log (Editor only).</summary>
    private const bool Enabled = false;

    private static string s_lastSelectionSummary = "(no_selection_event_yet)";
    private static readonly string s_logPath =
        Path.GetFullPath(Path.Combine(Application.dataPath, "..", "debug-4f0c84.log"));

    static WingsOfAshInspectorDebugLogger()
    {
        if (!Enabled)
        {
            return;
        }

        Selection.selectionChanged += OnSelectionChanged;
        Application.logMessageReceived += OnLogMessage;
    }

    [DidReloadScripts]
    private static void OnScriptsReloaded()
    {
        // #region agent log
        AppendNdjson(
            hypothesisId: "H3",
            location: "DidReloadScripts",
            message: "after_domain_reload",
            dataJson: $"\"summary\":{JsonEscape(BuildSelectionSummaryFresh())}");
        // #endregion
    }

    private static void OnSelectionChanged()
    {
        // #region agent log
        s_lastSelectionSummary = BuildSelectionSummaryFresh();
        AppendNdjson(
            hypothesisId: "H1",
            location: "Selection.selectionChanged",
            message: "selection_changed",
            dataJson: $"\"summary\":{JsonEscape(s_lastSelectionSummary)}");
        // #endregion
    }

    private static void OnLogMessage(string condition, string stackTrace, LogType type)
    {
        if (type != LogType.Error && type != LogType.Exception)
        {
            return;
        }

        bool serializedObj = condition.IndexOf("SerializedObjectNotCreatable", StringComparison.Ordinal) >= 0;
        bool inspectorNullRef = condition.IndexOf("NullReferenceException", StringComparison.Ordinal) >= 0 &&
                                  stackTrace != null &&
                                  stackTrace.IndexOf("GameObjectInspector", StringComparison.Ordinal) >= 0;
        bool inspector = serializedObj || inspectorNullRef;

        if (!inspector)
        {
            return;
        }

        // #region agent log
        string fresh = BuildSelectionSummaryFresh();
        AppendNdjson(
            hypothesisId: "H2",
            location: "Application.logMessageReceived",
            message: "unity_inspector_error",
            dataJson:
                $"\"condition\":{JsonEscape(condition)},\"stackOneLine\":{JsonEscape(FirstLine(stackTrace))},\"selectionAtError\":{JsonEscape(fresh)},\"cachedSelection\":{JsonEscape(s_lastSelectionSummary)}");
        // #endregion
    }

    private static string BuildSelectionSummaryFresh()
    {
        var ao = Selection.activeObject;
        var go = Selection.activeGameObject;
        var sb = new StringBuilder();
        sb.Append("selCount=").Append(Selection.instanceIDs.Length);

        if (ao == null)
        {
            sb.Append(";activeObject=null");
            return sb.ToString();
        }

        sb.Append(";activeType=").Append(ao.GetType().Name);
        sb.Append(";activeName=").Append(ao.name);

        string ap = AssetDatabase.GetAssetPath(ao);
        if (!string.IsNullOrEmpty(ap))
        {
            sb.Append(";assetPath=").Append(ap);
        }

        if (go == null)
        {
            sb.Append(";activeGameObject=null");
            return sb.ToString();
        }

        int missing = 0;
        var comps = go.GetComponents<Component>();
        for (int i = 0; i < comps.Length; i++)
        {
            if (comps[i] == null)
            {
                missing++;
            }
        }

        string prefabPath = PrefabUtility.IsPartOfPrefabInstance(go)
            ? PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(go)
            : "";

        sb.Append(";goPath=").Append(GetHierarchyPath(go.transform));
        sb.Append(";missingSlots=").Append(missing);
        if (!string.IsNullOrEmpty(prefabPath))
        {
            sb.Append(";prefabAssetPath=").Append(prefabPath);
        }

        return sb.ToString();
    }

    private static string GetHierarchyPath(Transform t)
    {
        var sb = new StringBuilder();
        while (t != null)
        {
            if (sb.Length > 0)
            {
                sb.Insert(0, "/");
            }

            sb.Insert(0, t.name);
            t = t.parent;
        }

        return sb.ToString();
    }

    private static string FirstLine(string s)
    {
        if (string.IsNullOrEmpty(s))
        {
            return "";
        }

        int n = s.IndexOf('\n');
        return n < 0 ? s : s.Substring(0, n);
    }

    private static void AppendNdjson(string hypothesisId, string location, string message, string dataJson)
    {
        long ts = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var line =
            "{\"sessionId\":\"4f0c84\",\"hypothesisId\":\"" + hypothesisId +
            "\",\"location\":\"" + location + "\",\"message\":\"" + message + "\",\"data\":{" + dataJson +
            "},\"timestamp\":" + ts + "}\n";
        try
        {
            File.AppendAllText(s_logPath, line);
        }
        catch
        {
            // ignore
        }
    }

    private static string JsonEscape(string s)
    {
        if (s == null)
        {
            return "\"\"";
        }

        return "\"" + s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r", " ").Replace("\n", " ") + "\"";
    }
}
#endif
