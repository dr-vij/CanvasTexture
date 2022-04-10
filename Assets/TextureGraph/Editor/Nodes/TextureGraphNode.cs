using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

[Serializable]
public class NodeSaveData
{
    public Vector2 Position;
    public NodeTypes NodeType;
}

[Serializable]
public class NodeDataContainer
{
    public ulong ID;
    public NodeSaveData NodeSaveData;
}

public abstract class TextureGraphNode : Node
{
    private GraphView m_GraphView;
    private Vector2 mPosition;

    public ulong ID { get; set; }

    public string NodeName { get; set; }

    public Vector2 Position
    {
        get => mPosition;
        set
        {
            mPosition = value;
            SetPosition(new Rect(value, Vector2.zero));
        }
    }

    public TextureGraphNode(GraphView graphView)
    {
        m_GraphView = graphView;
        NodeName = "Texture Node";
    }

    public abstract string Save();

    public abstract void Load(string data);

    public abstract NodeTypes NodeType { get; }

    public void Draw()
    {
        //override this methods to draw node correctly
        InitTitleContainer();
        InitPorts();
        InitExtensionContainer();

        RefreshExpandedState();
    }

    protected virtual void InitTitleContainer()
    {
        var textField = new TextField()
        {
            value = NodeName,
        };

        titleContainer.Insert(0, textField);
    }

    protected virtual void InitPorts() { }

    protected virtual void InitExtensionContainer() { }

    public void DisconnectAllPorts()
    {
        DisconnectInputPorts();
        DisconnectOutputPorts();
    }

    public void DisconnectInputPorts() => DisconnectPorts(inputContainer);

    public void DisconnectOutputPorts() => DisconnectPorts(outputContainer);

    private void DisconnectPorts(VisualElement container)
    {
        foreach (Port port in container.Children())
            if (port.connected)
                m_GraphView.DeleteElements(port.connections);
    }

    protected void AddInputPorts(params Port[] ports)
    {
        foreach (var port in ports)
        {
            inputContainer.Add(port);
        }
    }

    protected void AddOutputPorts(params Port[] ports)
    {
        foreach (var port in ports)
        {
            outputContainer.Add(port);
        }
    }
}
