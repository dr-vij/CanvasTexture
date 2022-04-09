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
        InitManipulators();
        InitBackground();
        InitUSS();
    }

    /// <summary>
    /// Adds new node
    /// </summary>
    private TextureGraphNode AddNode(Vector2 position)
    {
        var node = new TextureGraphNode();
        node.Init(position);
        node.Draw();
        return node;
    }

    /// <summary>
    /// Input and context menu manipulators
    /// </summary>
    private void InitManipulators()
    {
        SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());
        this.AddManipulator(CreateNodeContextMenu());
    }

    /// <summary>
    /// Context menu to create new nodes
    /// </summary>
    /// <returns></returns>
    private IManipulator CreateNodeContextMenu()
    {
        return new ContextualMenuManipulator(c => c.menu.AppendAction("Add node", act => AddElement(AddNode(act.eventInfo.mousePosition))));
    }

    /// <summary>
    /// Initialize background grid
    /// </summary>
    private void InitBackground()
    {
        var bg = new GridBackground();
        bg.StretchToParentSize();
        Insert(0, bg);
    }

    /// <summary>
    /// Initialize styles
    /// </summary>
    private void InitUSS()
    {
        var stylesheet = EditorGUIUtility.Load(GraphHelpers.PATH_TO_STYLES) as StyleSheet;
        if (stylesheet != null)
            styleSheets.Add(stylesheet);
        else
            Debug.LogWarning($"{GraphHelpers.PATH_TO_STYLES} was not found");
    }
}
