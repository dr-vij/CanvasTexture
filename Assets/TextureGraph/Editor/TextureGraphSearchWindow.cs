using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public enum NodeTypes
{
    GroupNode,
    TextureNode,
}


public class TextureGraphSearchWindow : ScriptableObject, ISearchWindowProvider
{
    private TextureGraphView m_Graph;
    private Texture2D m_indentationIcon;

    public void Init(TextureGraphView graph)
    {
        m_Graph = graph;
        m_indentationIcon = new Texture2D(1, 1);
        m_indentationIcon.SetPixel(0, 0, Color.clear);
        m_indentationIcon.Apply();
    }

    public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
    {
        return new List<SearchTreeEntry>()
        {
            new SearchTreeGroupEntry(new GUIContent("Create element", m_indentationIcon)),
            new SearchTreeGroupEntry(new GUIContent("Main Nodes",m_indentationIcon), 1),
            new SearchTreeEntry(new GUIContent("Color node",m_indentationIcon))
            {
                level = 2,
                userData = NodeTypes.TextureNode,
            },
            new SearchTreeEntry(new GUIContent("Group node", m_indentationIcon))
            {
                level = 1,
                userData = NodeTypes.GroupNode,
            },
        };
    }

    public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
    {
        var offset = -m_Graph.EditorWindow.position.position;
        var position = context.screenMousePosition + offset;
        switch (SearchTreeEntry.userData)
        {
            case NodeTypes.TextureNode:
                m_Graph.CreateColorNode(position);
                return true;
            case NodeTypes.GroupNode:
                m_Graph.CreateGroupNode("Group", position);
                return true;
            default:
                return false;
        }
    }
}
