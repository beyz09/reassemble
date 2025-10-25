#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

/// <summary>
/// Editor tool to automatically assign the `Clickable` component to GameObjects
/// that have an `Interactable` but are missing `Clickable`. Optionally it will
/// also add a simple BoxCollider2D or BoxCollider when none exists.
///
/// Usage: Window -> Clickable Assigner
/// Then choose options and press "Process Active Scene" or "Process Selected".
/// </summary>
public class ClickableAutoAssigner : EditorWindow
{
    bool addColliderIfMissing = true;
    bool prefer2D = true;
    bool processSelectedOnly = false;
    bool processPrefabs = false;
    string prefabFolder = "Assets";

    [MenuItem("Tools/Clickable Assigner")]
    public static void ShowWindow()
    {
        var w = GetWindow<ClickableAutoAssigner>("Clickable Assigner");
        w.minSize = new Vector2(360, 160);
    }

    void OnGUI()
    {
        GUILayout.Label("Automatically add Clickable to Interactable objects", EditorStyles.boldLabel);
        addColliderIfMissing = EditorGUILayout.Toggle(new GUIContent("Add Collider if missing","Add a BoxCollider2D/BoxCollider when the object has no collider"), addColliderIfMissing);
        prefer2D = EditorGUILayout.Toggle(new GUIContent("Prefer 2D colliders","When adding colliders prefer BoxCollider2D over BoxCollider"), prefer2D);
        processSelectedOnly = EditorGUILayout.Toggle(new GUIContent("Process Selected Only","If true, only process currently selected GameObjects in the Hierarchy"), processSelectedOnly);

        EditorGUILayout.Space();
        GUILayout.Label("Prefab scanning (optional)", EditorStyles.boldLabel);
        processPrefabs = EditorGUILayout.Toggle(new GUIContent("Process Prefabs","Also scan prefab assets under the given folder path"), processPrefabs);
        prefabFolder = EditorGUILayout.TextField(new GUIContent("Prefab folder","Root folder to scan for prefabs, e.g. Assets/Prefabs"), prefabFolder);

        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Process Active Scene", GUILayout.Height(32)))
        {
            ProcessActiveScene();
        }
        if (GUILayout.Button("Process Selected", GUILayout.Height(32)))
        {
            ProcessSelected();
        }
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Process Prefabs"))
        {
            ProcessPrefabs();
        }
    }

    void ProcessActiveScene()
    {
        var scene = SceneManager.GetActiveScene();
        if (!scene.isLoaded)
        {
            EditorUtility.DisplayDialog("Clickable Assigner", "No active loaded scene found.", "OK");
            return;
        }

        int added = 0;
        int collAdded = 0;
        var roots = scene.GetRootGameObjects();
        foreach (var root in roots)
        {
            ProcessRecursive(root, ref added, ref collAdded);
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorUtility.DisplayDialog("Clickable Assigner", $"Processing complete. Clickable added: {added}. Colliders added: {collAdded}.", "OK");
    }

    void ProcessSelected()
    {
        var selection = Selection.gameObjects;
        if (selection == null || selection.Length == 0)
        {
            EditorUtility.DisplayDialog("Clickable Assigner", "No GameObjects selected. Select items in the Hierarchy first.", "OK");
            return;
        }

        int added = 0;
        int collAdded = 0;
        foreach (var go in selection)
        {
            ProcessRecursive(go, ref added, ref collAdded);
        }

        EditorUtility.DisplayDialog("Clickable Assigner", $"Processing complete. Clickable added: {added}. Colliders added: {collAdded}.", "OK");
    }

    void ProcessPrefabs()
    {
        if (!processPrefabs)
        {
            if (!EditorUtility.DisplayDialog("Process Prefabs","You disabled prefab processing. Enable it in the window to run on prefabs.","Enable & Continue","Cancel"))
                return;
            processPrefabs = true;
        }

        string searchPath = string.IsNullOrEmpty(prefabFolder) ? "Assets" : prefabFolder;
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { searchPath });
        int added = 0;
        int collAdded = 0;
        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) continue;

            // Instantiate temporarily to edit safely
            var temp = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            if (temp == null) continue;

            ProcessRecursive(temp, ref added, ref collAdded);

            // Apply changes back to prefab
            PrefabUtility.SaveAsPrefabAssetAndConnect(temp, path, InteractionMode.UserAction);
            DestroyImmediate(temp);
        }

        AssetDatabase.SaveAssets();
        EditorUtility.DisplayDialog("Clickable Assigner", $"Prefab processing complete. Clickable added: {added}. Colliders added: {collAdded}.", "OK");
    }

    void ProcessRecursive(GameObject go, ref int added, ref int collAdded)
    {
        if (go == null) return;

        // If it has an Interactable-derived component
        var interact = go.GetComponent<Interactable>();
        if (interact != null)
        {
            var clickable = go.GetComponent<Clickable>();
            if (clickable == null)
            {
                Undo.AddComponent<Clickable>(go);
                added++;
                Debug.Log("Added Clickable to: " + GetFullPath(go));
            }

            if (addColliderIfMissing)
            {
                bool has2D = go.GetComponent<Collider2D>() != null;
                bool has3D = go.GetComponent<Collider>() != null;
                if (!has2D && !has3D)
                {
                    if (prefer2D)
                    {
                        Undo.AddComponent<BoxCollider2D>(go);
                        collAdded++;
                        Debug.Log("Added BoxCollider2D to: " + GetFullPath(go));
                    }
                    else
                    {
                        Undo.AddComponent<BoxCollider>(go);
                        collAdded++;
                        Debug.Log("Added BoxCollider to: " + GetFullPath(go));
                    }
                }
            }
        }

        // Recurse children
        for (int i = 0; i < go.transform.childCount; i++)
        {
            var child = go.transform.GetChild(i).gameObject;
            ProcessRecursive(child, ref added, ref collAdded);
        }
    }

    string GetFullPath(GameObject go)
    {
        string path = go.name;
        var t = go.transform;
        while (t.parent != null)
        {
            t = t.parent;
            path = t.name + "/" + path;
        }
        return path;
    }
}
#endif
