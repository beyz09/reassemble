using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Editor utility to auto-bind DialogueUI fields in the currently open scene.
/// Heuristics:
/// - panel: finds a GameObject with "dialog"/"panel" in the name that contains a Text child
/// - lineText: finds first Text under panel or in scene
/// - choicesContainer: finds RectTransform named "choices"/"buttons" or with a LayoutGroup
/// 
/// Menu: Tools/Dialogue UI/Auto Bind DialogueUI in Scene
/// </summary>
public static class DialogueUIAutoBinder
{
    [MenuItem("Tools/Dialogue UI/Auto Bind DialogueUI in Scene")]
    public static void AutoBindAll()
    {
        var all = Object.FindObjectsOfType<DialogueUI>(true);
        int boundCount = 0;
        foreach (var du in all)
        {
            if (AutoBind(du)) boundCount++;
        }
        EditorUtility.DisplayDialog("Auto Bind DialogueUI", $"Processed {all.Length} DialogueUI(s). Bound: {boundCount}", "OK");
    }

    public static bool AutoBind(DialogueUI du)
    {
        if (du == null) return false;
        bool changed = false;

        // Find panel
        if (du.panel == null)
        {
            var root = du.gameObject.scene.GetRootGameObjects();
            GameObject foundPanel = null;
            foreach (var r in root)
            {
                foundPanel = FindPanelUnder(r.transform);
                if (foundPanel != null) break;
            }
            if (foundPanel != null)
            {
                du.panel = foundPanel;
                changed = true;
            }
        }

        // lineText
        if (du.lineText == null)
        {
            var t2 = FindFirstComponentInPanelOrScene<Text>(du.panel);
            if (t2 != null) { du.lineText = t2; changed = true; }
        }

        // choicesContainer
        if (du.choicesContainer == null)
        {
            RectTransform chosen = null;
            if (du.panel != null)
            {
                chosen = FindChoicesContainer(du.panel.transform);
            }
            if (chosen == null)
            {
                var root = du.gameObject.scene.GetRootGameObjects();
                foreach (var r in root)
                {
                    chosen = FindChoicesContainer(r.transform);
                    if (chosen != null) break;
                }
            }
            if (chosen != null)
            {
                du.choicesContainer = chosen;
                changed = true;
            }
        }

        if (changed)
        {
            EditorUtility.SetDirty(du);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(du.gameObject.scene);
        }

        Debug.Log($"DialogueUI AutoBind for '{du.name}': panel={(du.panel!=null)} lineText={(du.lineText!=null)} choices={(du.choicesContainer!=null)}");
        return changed;
    }

    static GameObject FindPanelUnder(Transform t)
    {
        if (t.GetComponent<RectTransform>() != null)
        {
            var name = t.name.ToLowerInvariant();
            if (name.Contains("dialog") || name.Contains("panel") || name.Contains("ui"))
            {
                if (t.GetComponentInChildren<Text>(true) != null)
                    return t.gameObject;
            }
        }
        foreach (Transform c in t)
        {
            var g = FindPanelUnder(c);
            if (g != null) return g;
        }
        return null;
    }

    static T FindFirstComponentInPanelOrScene<T>(GameObject panel) where T: Component
    {
        if (panel != null)
        {
            var comp = panel.GetComponentInChildren<T>(true);
            if (comp != null) return comp;
        }
        var all = Object.FindObjectsOfType<T>(true);
        if (all != null && all.Length>0) return all[0];
        return null;
    }

    static RectTransform FindChoicesContainer(Transform t)
    {
        var name = t.name.ToLowerInvariant();
        if (name.Contains("choice") || name.Contains("choices") || name.Contains("buttons") || name.Contains("answers"))
        {
            var rt = t.GetComponent<RectTransform>();
            if (rt != null) return rt;
        }
        if (t.GetComponent<UnityEngine.UI.LayoutGroup>() != null)
        {
            var rt = t.GetComponent<RectTransform>();
            if (rt != null) return rt;
        }
        foreach (Transform c in t)
        {
            var found = FindChoicesContainer(c);
            if (found != null) return found;
        }
        return null;
    }
}
