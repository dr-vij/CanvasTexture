using System.Collections;
using System.Collections.Generic;
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
        mGraph.graphElements.ForEach(element =>
        {
            if (element is TextureGraphNode node)
            {
                Debug.Log(node.Save());
            }
        });
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
