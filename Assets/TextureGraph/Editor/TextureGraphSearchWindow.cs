using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public enum NodeTypes
{
    ColorNode,
    ImageNode,
}

public class GroupType
{
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
            CreateSearchEntry("Color Node", NodeTypes.ColorNode),
            CreateSearchEntry("Image Node", NodeTypes.ImageNode),

            CreateSearchEntry("Group", typeof(GroupType), 1),
        };
    }

    public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
    {
        var offset = -m_Graph.EditorWindow.position.position;
        var position = context.screenMousePosition + offset;
        switch (SearchTreeEntry.userData)
        {
            case NodeTypes.ImageNode:
                m_Graph.CreateNode(NodeTypes.ImageNode, position);
                return true;
            case NodeTypes.ColorNode:
                m_Graph.CreateNode(NodeTypes.ColorNode, position);
                return true;
            case Type type:
                {
                    if (type == typeof(GroupType))
                        m_Graph.CreateGroupNode("Group", position);
                    else
                        Debug.LogError("Unknow node type");
                    return true;
                }
            default:
                return false;
        }
    }

    private SearchTreeEntry CreateSearchEntry(string label, NodeTypes type, int level = 2)
    {
        return new SearchTreeEntry(new GUIContent(label, m_indentationIcon))
        {
            level = level,
            userData = type,
        };
    }

    private SearchTreeEntry CreateSearchEntry(string label, Type type, int level = 1)
    {
        return new SearchTreeEntry(new GUIContent(label, m_indentationIcon))
        {
            level = level,
            userData = type,
        };
    }
}
