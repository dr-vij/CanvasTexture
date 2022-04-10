using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public static class GraphHelpers
{
    public const string PATH_TO_STYLES = "TextureGraphStyles.uss";
}

public class TextureEditorWindow : EditorWindow
{
    private TextureGraphView mGraph;

    [MenuItem("Windows/TextureGraph")]
    public static void Open()
    {
        GetWindow<TextureEditorWindow>("Texture graph");
    }

    private void OnEnable()
    {
        AddGraph();
        AddToolbar();
        AddStyles();
    }

    /// <summary>
    /// Adds toolbar with save load buttons and etc.
    /// </summary>
    private void AddToolbar()
    {
        var toolbar = new Toolbar();

        var saveButton = new Button(() => Save());
        saveButton.text = "Save";
        toolbar.Add(saveButton);

        rootVisualElement.Add(toolbar);
    }

    private void Save()
    {
        var nodesToSave = new List<NodeSaveData>();
        mGraph.graphElements.ForEach(element =>
        {
            if (element is TextureGraphNode node)
            {
                nodesToSave.Add(node.Save());
            }
        });

        var so = CreateAsset<TextureGraph>("Assets", "test");
        so.Save(nodesToSave);
        SaveAsset(so);
    }

    public static T CreateAsset<T>(string folder, string name) where T : ScriptableObject
    {
        var path = Path.Combine(folder, $"{name}.asset");
        var asset = LoadAsset<T>(path);
        if (asset == null)
        {
            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
        }
        return asset;
    }

    public static T LoadAsset<T>(string path) where T : ScriptableObject
    {
        return AssetDatabase.LoadAssetAtPath<T>(path);
    }

    public static void SaveAsset(Object asset)
    {
        EditorUtility.SetDirty(asset);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    /// <summary>
    /// Adds graph itself
    /// </summary>
    private void AddGraph()
    {
        mGraph = new TextureGraphView(this);
        mGraph.StretchToParentSize();
        rootVisualElement.Add(mGraph);
    }

    /// <summary>
    /// Adds graph styles
    /// </summary>
    private void AddStyles()
    {
        var stylesheet = EditorGUIUtility.Load(GraphHelpers.PATH_TO_STYLES) as StyleSheet;
        if (stylesheet != null)
            rootVisualElement.styleSheets.Add(stylesheet);
        else
            Debug.LogWarning($"{GraphHelpers.PATH_TO_STYLES} was not found");
    }
}
