using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class IconGeneration : MonoBehaviour
{
    [Header("Scene Objects (one at a time)")]
    public List<GameObject> sceneObjects;

    [Header("Linked ScriptableObjects")]
    public List<InventoryItemData> dataObjects;

    [Header("Camera Settings")]
    public Camera iconCamera;
    public int iconSize = 256;

    [Header("Output Settings")]
    public string pathFolder = "GameItems/ItemSprites";
    public string fileExtension = ".png";

    private void Awake()
    {
        if (iconCamera == null)
            iconCamera = GetComponent<Camera>();

        // Force white background
        iconCamera.clearFlags = CameraClearFlags.SolidColor;
        iconCamera.backgroundColor = Color.white;
    }

    [ContextMenu("Generate Icons")]
    public void GenerateIcons()
    {
        StartCoroutine(ScreenshotSequence());
    }

    private IEnumerator ScreenshotSequence()
    {
        if (sceneObjects.Count != dataObjects.Count)
        {
            Debug.LogError("Scene objects and dataObjects must be the same length!");
            yield break;
        }

        for (int i = 0; i < sceneObjects.Count; i++)
        {
            GameObject obj = sceneObjects[i];
            InventoryItemData data = dataObjects[i];

            obj.SetActive(true);
            yield return null; // wait one frame

            // Screenshot path
            string fullPath = $"{Application.dataPath}/{pathFolder}/{data.id}_Icon{fileExtension}";
            TakeScreenshot(fullPath);

            yield return null;

            obj.SetActive(false);

#if UNITY_EDITOR
            // Load PNG as sprite
            Sprite s = AssetDatabase.LoadAssetAtPath<Sprite>(
                $"Assets/{pathFolder}/{data.id}_Icon{fileExtension}"
            );

            if (s != null)
            {
                data.icon = s;
                EditorUtility.SetDirty(data);
            }
#endif

            yield return null; // allow icon update
        }

#if UNITY_EDITOR
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
#endif

        Debug.Log("Finished generating icons.");
    }

    private void TakeScreenshot(string fullPath)
    {
        int width = iconSize;
        int height = iconSize;

        RenderTexture rt = new RenderTexture(width, height, 24);
        iconCamera.targetTexture = rt;

        Texture2D image = new Texture2D(width, height, TextureFormat.RGBA32, false);

        // White background for all pipelines
        RenderTexture.active = rt;
        GL.Clear(true, true, Color.white);

        iconCamera.Render();
        image.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        image.Apply();

        iconCamera.targetTexture = null;
        RenderTexture.active = null;

#if UNITY_EDITOR
        DestroyImmediate(rt);
#else
        Destroy(rt);
#endif

        File.WriteAllBytes(fullPath, image.EncodeToPNG());
        Debug.Log("Saved icon: " + fullPath);
    }
}
