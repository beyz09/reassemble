using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Editor utility to auto-wire the Level5 CatWash scene based on common object names.
/// Finds objects like 'cat1', 'soap', 'towel', 'water', 'LevelManager', 'Canvas/PatienceBar_BG' and attaches components
/// and connects UnityEvents so the level is ready to test in editor.
/// </summary>
public static class Level5AutoWire
{
    [MenuItem("Tools/Level5/Auto Wire CatWash")]
    public static void AutoWire()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            Debug.LogError("AutoWire cannot run in play mode. Please stop Play Mode and run again.");
            return;
        }
        // find root objects
        GameObject cat = GameObject.Find("cat1") ?? GameObject.Find("Cat1") ?? GameObject.Find("cat");
        GameObject soap = GameObject.Find("soap") ?? GameObject.Find("Soap");
        GameObject towel = GameObject.Find("towel") ?? GameObject.Find("Towel");
        GameObject water = GameObject.Find("water") ?? GameObject.Find("Water");
        GameObject levelManager = GameObject.Find("LevelManager") ?? GameObject.Find("levelmanager");
        GameObject canvas = GameObject.Find("Canvas");
        GameObject patienceBg = null;
        if (canvas != null)
        {
            var child = canvas.transform.Find("PatienceBar_BG");
            if (child != null) patienceBg = child.gameObject;
        }

        if (levelManager == null)
        {
            Debug.LogError("LevelManager object not found — please ensure a GameObject named 'LevelManager' exists.");
            return;
        }

        // Ensure EventSystem exists (required for IPointer events)
        if (GameObject.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var es = new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(UnityEngine.EventSystems.StandaloneInputModule));
            UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
            Debug.Log("EventSystem created for pointer events.");
        }

        // Ensure main camera has a Physics2DRaycaster so SpriteRenderer objects with Collider2D receive pointer events
        Camera mainCam = Camera.main ?? GameObject.FindObjectOfType<Camera>();
        if (mainCam != null)
        {
            if (mainCam.GetComponent<UnityEngine.EventSystems.Physics2DRaycaster>() == null)
            {
                mainCam.gameObject.AddComponent<UnityEngine.EventSystems.Physics2DRaycaster>();
                Debug.Log("Added Physics2DRaycaster to main camera to support 2D pointer events.");
            }
        }

        // ensure Level5_CatWashManager exists on levelManager
        var manager = levelManager.GetComponent<Level5_CatWashManager>();
        if (manager == null)
        {
            manager = levelManager.AddComponent<Level5_CatWashManager>();
        }

        // placeHold: on cat object (add DragSensitiveHold)
        DragSensitiveHold placeHold = null;
        if (cat != null)
        {
            placeHold = cat.GetComponent<DragSensitiveHold>() ?? cat.AddComponent<DragSensitiveHold>();
            placeHold.requiredHoldTime = 3f;
            placeHold.aggressiveSpeedThreshold = 900f;
            // ensure there is a Collider2D so pointer raycasts hit this sprite
            var sr = cat.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                if (cat.GetComponent<Collider2D>() == null)
                {
                    var box = cat.AddComponent<BoxCollider2D>();
                    box.isTrigger = true;
                    Debug.Log("Added BoxCollider2D to cat for pointer interactions.");
                }
            }
            else
            {
                // if it's UI, ensure raycast target
                var img = cat.GetComponent<UnityEngine.UI.Image>();
                if (img != null) img.raycastTarget = true;
            }
        }

        // add DragDropPlace to cat to support drag-and-drop placement
        DragDropPlace placeDrop = null;
        if (cat != null)
        {
            placeDrop = cat.GetComponent<DragDropPlace>() ?? cat.AddComponent<DragDropPlace>();
            // prefer explicit tub if we found one
            GameObject tubFound = GameObject.Find("toob1") ?? GameObject.Find("toob2") ?? GameObject.Find("toob") ?? GameObject.Find("tub") ?? GameObject.Find("bathtub") ?? GameObject.Find("bath");
            if (tubFound != null) placeDrop.dropTarget = tubFound.transform;
            // add visual follow so the cat visibly follows the pointer while dragging
            var visual = cat.GetComponent<DragVisualFollow>() ?? cat.AddComponent<DragVisualFollow>();
        }

        // foamAction: on soap object
        FoamAction foam = null;
        if (soap != null)
        {
            foam = soap.GetComponent<FoamAction>() ?? soap.AddComponent<FoamAction>();
            foam.requiredTime = 10f;
            foam.aggressiveSpeedThreshold = 900f;
            var sr = soap.GetComponent<SpriteRenderer>();
            if (sr != null && soap.GetComponent<Collider2D>() == null)
            {
                var box = soap.AddComponent<BoxCollider2D>();
                box.isTrigger = true;
                Debug.Log("Added BoxCollider2D to soap for pointer interactions.");
            }
            else
            {
                var img = soap.GetComponent<UnityEngine.UI.Image>();
                if (img != null) img.raycastTarget = true;
            }
            // add visual follow so soap visibly follows the pointer while dragging
            var soapVis = soap.GetComponent<DragVisualFollow>() ?? soap.AddComponent<DragVisualFollow>();
        }

        // rinse/dry manager
        RinseAndDryAction rinseDry = levelManager.GetComponent<RinseAndDryAction>() ?? levelManager.AddComponent<RinseAndDryAction>();
        rinseDry.allowedSlips = 1;

        // add DragSensitiveHold to towel and water and wire to rinseDry
        if (towel != null)
        {
            var dh = towel.GetComponent<DragSensitiveHold>() ?? towel.AddComponent<DragSensitiveHold>();
            dh.requiredHoldTime = 5f;
            dh.aggressiveSpeedThreshold = 900f;
            // ensure collider or UI raycast target
            var srT = towel.GetComponent<SpriteRenderer>();
            if (srT != null && towel.GetComponent<Collider2D>() == null)
            {
                var box = towel.AddComponent<BoxCollider2D>();
                box.isTrigger = true;
                Debug.Log("Added BoxCollider2D to towel for pointer interactions.");
            }
            else
            {
                var img = towel.GetComponent<UnityEngine.UI.Image>();
                if (img != null) img.raycastTarget = true;
            }
            // add visual follow for towel so dragging is visible
            var towVis = towel.GetComponent<DragVisualFollow>() ?? towel.AddComponent<DragVisualFollow>();
            // towel will be used later for final drying; manager will wire it at runtime
        }

        if (water != null)
        {
            var dh = water.GetComponent<DragSensitiveHold>() ?? water.AddComponent<DragSensitiveHold>();
            dh.requiredHoldTime = 5f;
            dh.aggressiveSpeedThreshold = 900f;
            var srW = water.GetComponent<SpriteRenderer>();
            if (srW != null && water.GetComponent<Collider2D>() == null)
            {
                var box = water.AddComponent<BoxCollider2D>();
                box.isTrigger = true;
                Debug.Log("Added BoxCollider2D to water for pointer interactions.");
            }
            else
            {
                var img = water.GetComponent<UnityEngine.UI.Image>();
                if (img != null) img.raycastTarget = true;
            }
            // add visual follow for water
            var watVis = water.GetComponent<DragVisualFollow>() ?? water.AddComponent<DragVisualFollow>();
            // water will be used for rinsing; manager will wire it at runtime
        }

        // try to find a tub/bath object and create BathTubController wiring
        GameObject tub1 = GameObject.Find("toob1");
        GameObject tub2 = GameObject.Find("toob2");
        GameObject tub = tub1 ?? GameObject.Find("toob") ?? GameObject.Find("tub") ?? GameObject.Find("bathtub") ?? GameObject.Find("bath");
        // If separate toob2 exists and tub1 was found, prefer tub1 as empty and tub2 as the 'with-cat' variant
        if (tub == null && tub2 != null) tub = tub2;

        if (tub != null)
        {
            var bathCtrl = tub.GetComponent<BathTubController>() ?? tub.AddComponent<BathTubController>();
            bathCtrl.tubEmpty = tub;
            // attempt to find tub-with-cat variant: prefer explicit toob2 if it exists and is different from tub
            GameObject tubWithCat = null;
            if (tub1 != null && tub2 != null && tub != tub2)
            {
                tubWithCat = tub2;
            }
            else
            {
                tubWithCat = GameObject.Find(tub.name + "_cat") ?? GameObject.Find(tub.name + "cat") ?? GameObject.Find("toob1_cat") ?? GameObject.Find("toob_cat") ?? GameObject.Find("tub_with_cat") ?? tub2;
            }

            if (tubWithCat != null && tubWithCat != tub)
            {
                bathCtrl.tubWithCat = tubWithCat;
                tubWithCat.SetActive(false);
            }
            // assign cat reference if found
            if (cat != null) bathCtrl.catObject = cat;
            // try find wet cat variant
            GameObject catWet = GameObject.Find((cat != null ? cat.name + "_wet" : "cat_wet")) ?? GameObject.Find("cat_wet");
            if (catWet != null)
            {
                bathCtrl.catWetObject = catWet;
                catWet.SetActive(false);
            }

            // wire placeHold success to show tub-with-cat
            if (placeHold != null)
            {
                UnityEventTools.AddPersistentListener(placeHold.OnSuccessEvent, new UnityEngine.Events.UnityAction(bathCtrl.ShowTubWithCat));
            }
            // also wire drop-based placement if present
            if (placeDrop != null)
            {
                UnityEventTools.AddPersistentListener(placeDrop.OnSuccessEvent, new UnityEngine.Events.UnityAction(bathCtrl.ShowTubWithCat));
            }
            // wire rinse/dry completion to make cat wet
            if (rinseDry != null)
            {
                UnityEventTools.AddPersistentListener(rinseDry.OnSuccessEvent, new UnityEngine.Events.UnityAction(bathCtrl.MakeCatWet));
            }
        }

        // assign references on manager (serialized assignment)
        SerializedObject so = new SerializedObject(manager);
        if (placeHold != null)
        {
            var prop = so.FindProperty("placeHold");
            prop.objectReferenceValue = placeHold;
        }
        if (placeDrop != null)
        {
            var prop2 = so.FindProperty("placeDrop");
            prop2.objectReferenceValue = placeDrop;
        }
        if (foam != null)
        {
            var prop = so.FindProperty("foamAction");
            prop.objectReferenceValue = foam;
        }
        if (rinseDry != null)
        {
            var prop = so.FindProperty("rinseDryAction");
            prop.objectReferenceValue = rinseDry;
        }
        // assign water/towel holds and bathController if present
        // water
        if (water != null)
        {
            var prop = so.FindProperty("waterHold");
            prop.objectReferenceValue = water.GetComponent<DragSensitiveHold>();
        }
        // towel
        if (towel != null)
        {
            var prop = so.FindProperty("towelHold");
            prop.objectReferenceValue = towel.GetComponent<DragSensitiveHold>();
        }
        // bath controller
        if (tub != null)
        {
            var prop = so.FindProperty("bathController");
            prop.objectReferenceValue = tub.GetComponent<BathTubController>();
        }
        so.ApplyModifiedProperties();

        // Setup patience bar UI and hook foam progress
        if (patienceBg != null)
        {
            // try find a child named PatienceBar_Fill
            Transform fillT = patienceBg.transform.Find("PatienceBar_Fill");
            GameObject fillObj;
            if (fillT == null)
            {
                fillObj = new GameObject("PatienceBar_Fill", typeof(RectTransform), typeof(Image));
                fillObj.transform.SetParent(patienceBg.transform, false);
                var img = fillObj.GetComponent<Image>();
                img.type = Image.Type.Filled;
                img.fillMethod = Image.FillMethod.Horizontal;
                img.fillAmount = 1f;
            }
            else fillObj = fillT.gameObject;

            var controller = patienceBg.GetComponent<UIFoamBarController>() ?? patienceBg.AddComponent<UIFoamBarController>();
            controller.fillImage = fillObj.GetComponent<Image>();

            if (foam != null)
            {
                UnityEventTools.AddPersistentListener(foam.OnProgress, new UnityEngine.Events.UnityAction<float>(controller.SetProgress));
            }
        }

        // mark scene dirty
        UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
        Debug.Log("Level5 auto-wire completed. Check LevelManager inspector for references and test the scene.");
    }
}
