using UnityEngine;
using UnityEditor;

/// <summary>
/// Small editor utility: create a Background GameObject from the selected Sprite asset.
/// Usage: select a sprite in the Project window, then Tools -> Background -> Create Background From Selected Sprite
/// It will also ensure a Sorting Layer named "Background" exists and set Order in Layer to -10.
/// </summary>
public static class BackgroundCreator
{
    [MenuItem("Tools/Background/Create Background From Selected Sprite", priority = 100)]
    static void CreateBackgroundFromSelection()
    {
        Sprite selectedSprite = Selection.activeObject as Sprite;

        // If user selected the texture (Texture2D) instead of the Sprite sub-asset, try to extract the Sprite
        if (selectedSprite == null)
        {
            var tex = Selection.activeObject as Texture2D;
            if (tex != null)
            {
                string path = AssetDatabase.GetAssetPath(tex);
                var assets = AssetDatabase.LoadAllAssetsAtPath(path);
                if (assets != null)
                {
                    foreach (var a in assets)
                    {
                        if (a is Sprite s)
                        {
                            selectedSprite = s;
                            break;
                        }
                    }
                }
            }
        }

        if (selectedSprite == null)
        {
            EditorUtility.DisplayDialog("Create Background", "Please select a Sprite asset (or the Texture2D of a sprite) in the Project window.", "OK");
            return;
        }

        const string layerName = "Background";
        EnsureSortingLayerExists(layerName);

        GameObject bg = new GameObject("Background");
    var sr = bg.AddComponent<SpriteRenderer>();
    sr.sprite = selectedSprite;
        sr.sortingLayerName = layerName;
        sr.sortingOrder = -10;

        // Place at origin. Adjust Z so camera can see it if needed.
        bg.transform.position = Vector3.zero;

        // Select and ping the created object
        Selection.activeGameObject = bg;
        EditorGUIUtility.PingObject(bg);
    }

    static void EnsureSortingLayerExists(string layer)
    {
        // Load TagManager and check m_SortingLayers
        var assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
        if (assets == null || assets.Length == 0) return;

        SerializedObject tagManager = new SerializedObject(assets[0]);
        SerializedProperty sortingLayers = tagManager.FindProperty("m_SortingLayers");
        if (sortingLayers == null) return;

        for (int i = 0; i < sortingLayers.arraySize; i++)
        {
            var elem = sortingLayers.GetArrayElementAtIndex(i);
            var nameProp = elem.FindPropertyRelative("name");
            if (nameProp != null && nameProp.stringValue == layer)
                return; // already exists
        }

        // Add a new sorting layer entry
        sortingLayers.InsertArrayElementAtIndex(sortingLayers.arraySize);
        var newElem = sortingLayers.GetArrayElementAtIndex(sortingLayers.arraySize - 1);
        var newName = newElem.FindPropertyRelative("name");
        if (newName != null) newName.stringValue = layer;

        tagManager.ApplyModifiedProperties();
    }
}
