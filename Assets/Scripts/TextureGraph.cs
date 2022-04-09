using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class TextureGraphView : GraphView
{
    public TextureGraphView()
    {
        InitBg();
        AddStyles();
    }

    private void InitBg()
    {
        var bg = new GridBackground();
        bg.StretchToParentSize();
        Insert(0, bg);
    }

    private void AddStyles()
    {
        var stylesheet = EditorGUIUtility.Load(GraphHelpers.PATH_TO_STYLES) as StyleSheet;
        if (stylesheet != null)
            styleSheets.Add(stylesheet);
        else
            Debug.LogWarning($"{GraphHelpers.PATH_TO_STYLES} was not found");
    }
}
