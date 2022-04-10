using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public static class GraphHelpers
{
    public const string PATH_TO_STYLES = "TextureGraphStyles.uss";
}

public class TextureEditorWindow : EditorWindow
{
    [MenuItem("Windows/TextureGraph")]
    public static void Open()
    {
        GetWindow<TextureEditorWindow>("Texture graph");
    }

    private void OnEnable()
    {
        AddGraph();
        AddStyles();
    }

    private void AddGraph()
    {
        var graph = new TextureGraphView();
        graph.StretchToParentSize();
        rootVisualElement.Add(graph);
    }

    private void AddStyles()
    {
        var stylesheet = EditorGUIUtility.Load(GraphHelpers.PATH_TO_STYLES) as StyleSheet;
        if (stylesheet != null)
            rootVisualElement.styleSheets.Add(stylesheet);
        else
            Debug.LogWarning($"{GraphHelpers.PATH_TO_STYLES} was not found");
    }
}
