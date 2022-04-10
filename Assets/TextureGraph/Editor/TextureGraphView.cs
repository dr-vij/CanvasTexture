using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class TextureGraphView : GraphView
{
    private TextureGraphSearchWindow m_SearchWindow;
    private EditorWindow mEditorWindow;

    public EditorWindow EditorWindow => mEditorWindow;

    public TextureGraphView(EditorWindow owner)
    {
        mEditorWindow = owner;

        InitManipulators();
        InitBackground();
        InitStyles();
        CreateSearchWindow();
        InitNodeDeleteCallback();
    }

    #region Node creation

    public Node CreateNode(NodeTypes nodeType, Vector2 worldPosition)
    {
        var localPosition = GetLocalMousePosition(worldPosition);
        TextureGraphNode node = null;
        switch (nodeType)
        {
            case NodeTypes.ColorNode:
                node = new ColorNode(this);
                break;
            case NodeTypes.ImageNode:
                node = new ImageNode(this);
                break;
        }
        node.Position = localPosition;
        node.Draw();
        AddElement(node);
        return node;
    }

    public TextureGraphGroup CreateGroupNode(string groupTitle, Vector2 position)
    {
        var group = new TextureGraphGroup(groupTitle, position);
        foreach (var selected in selection)
            if (selected is Node node)
                group.AddElement(node);

        AddElement(group);
        return group;
    }

    #endregion

    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
    {
        var compatablePorts = new List<Port>();
        ports.ForEach(port =>
        {
            var isSameNodes = startPort.node == port.node;
            var isSameDirections = startPort.direction == port.direction;
            var isSameTypes = startPort.portType == port.portType;

            if (!isSameNodes && !isSameDirections && isSameTypes)
            {
                compatablePorts.Add(port);
            }
        });
        return compatablePorts;
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

        //Context menu to create new nodes
        //this.AddManipulator(new ContextualMenuManipulator(c => c.menu.AppendAction("Add node", act => AddElement(CreateColorNode(act.eventInfo.mousePosition)))));
        this.AddManipulator(new ContextualMenuManipulator(c => c.menu.AppendAction("Add group", act => AddElement(CreateGroupNode("Group", act.eventInfo.mousePosition)))));
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
    private void InitStyles()
    {
        var stylesheet = EditorGUIUtility.Load(GraphHelpers.PATH_TO_STYLES) as StyleSheet;
        if (stylesheet != null)
            styleSheets.Add(stylesheet);
        else
            Debug.LogWarning($"{GraphHelpers.PATH_TO_STYLES} was not found");
    }

    private void CreateSearchWindow()
    {
        if (m_SearchWindow == null)
        {
            m_SearchWindow = ScriptableObject.CreateInstance<TextureGraphSearchWindow>();
            m_SearchWindow.Init(this);
        }
        nodeCreationRequest = context => SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), m_SearchWindow);
    }

    /// <summary>
    /// This method overrides callback for node deletion
    /// </summary>
    private void InitNodeDeleteCallback()
    {
        deleteSelection = (operationName, askUser) =>
        {
            var nodesToDelete = new List<TextureGraphNode>();
            var groupsToDelete = new List<Group>();
            var edgesToDelete = new List<Edge>();
            foreach (var selectable in selection)
            {
                switch (selectable)
                {
                    case TextureGraphNode node:
                        nodesToDelete.Add(node);
                        break;
                    case Group group:
                        groupsToDelete.Add(group);
                        break;
                    case Edge edge:
                        edgesToDelete.Add(edge);
                        break;
                }
            }

            foreach (var group in groupsToDelete)
                foreach (var element in group.containedElements)
                    if (element is TextureGraphNode node)
                        nodesToDelete.Add(node);

            foreach (var node in nodesToDelete)
            {
                node.DisconnectAllPorts();
                RemoveElement(node);
            }

            DeleteElements(groupsToDelete);
            DeleteElements(edgesToDelete);
        };
    }

    public Vector2 GetLocalMousePosition(Vector2 mousePosition) => contentViewContainer.WorldToLocal(mousePosition);
}
