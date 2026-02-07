using UnityEngine;
using UnityEditor;

public class CopySpriteSlicing : EditorWindow
{
    public Texture2D sourceTexture;
    public Texture2D targetTexture;

    [MenuItem("Tools/Copy Sprite Slicing")]
    public static void ShowWindow()
    {
        GetWindow<CopySpriteSlicing>("Copy Slicing");
    }

    void OnGUI()
    {
        GUILayout.Label("Copy Sprite Slicing & Naming", EditorStyles.boldLabel);
        GUILayout.Space(10);

        sourceTexture = (Texture2D)EditorGUILayout.ObjectField("Source (Good Slicing)", sourceTexture, typeof(Texture2D), false);
        targetTexture = (Texture2D)EditorGUILayout.ObjectField("Target (Needs Slicing)", targetTexture, typeof(Texture2D), false);

        GUILayout.Space(20);

        if (GUILayout.Button("Copy Slicing Data"))
        {
            if (sourceTexture != null && targetTexture != null)
            {
                CopySlicingData(sourceTexture, targetTexture);
            }
            else
            {
                Debug.LogError("Please assign both Source and Target textures.");
            }
        }
    }

    private void CopySlicingData(Texture2D source, Texture2D target)
    {
        string sourcePath = AssetDatabase.GetAssetPath(source);
        string targetPath = AssetDatabase.GetAssetPath(target);

        TextureImporter sourceImporter = AssetImporter.GetAtPath(sourcePath) as TextureImporter;
        TextureImporter targetImporter = AssetImporter.GetAtPath(targetPath) as TextureImporter;

        if (sourceImporter == null || targetImporter == null)
        {
            Debug.LogError("Could not load TextureImporters.");
            return;
        }

        targetImporter.spriteImportMode = sourceImporter.spriteImportMode;
        targetImporter.spritePixelsPerUnit = sourceImporter.spritePixelsPerUnit;

        var sourceSheet = sourceImporter.spritesheet;
        var targetSheet = new SpriteMetaData[sourceSheet.Length];

        for (int i = 0; i < sourceSheet.Length; i++)
        {
            targetSheet[i] = sourceSheet[i];
        }

        targetImporter.spritesheet = targetSheet;

        EditorUtility.SetDirty(targetImporter);
        targetImporter.SaveAndReimport();

        Debug.Log($"<color=green>Success!</color> Copied {targetSheet.Length} sprites from {source.name} to {target.name}.");
    }
}
