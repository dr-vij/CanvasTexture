using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

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
                userData = "",
            }
        };

    }

    public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
    {
        var offset = -m_Graph.EditorWindow.position.position;

        switch (SearchTreeEntry.userData)
        {
            case string str:
                m_Graph.CreateTextureNode(context.screenMousePosition + offset);
                return true;
            default:
                return false;
        }
    }
}
