using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public abstract class TextureGraphNode : Node
{
    private GraphView m_GraphView;

    public string NodeName { get; set; }

    public TextureGraphNode(Vector2 position, GraphView graphView)
    {
        m_GraphView = graphView;
        NodeName = "Texture Node";
        SetPosition(new Rect(position, Vector2.zero));
    }

    //public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
    //{
    //    evt.menu.AppendAction("Disconnect inputs", actionEvent => DisconnectInputPorts());
    //    evt.menu.AppendAction("Disconnect outputs", actionEvent => DisconnectOutputPorts());

    //    base.BuildContextualMenu(evt);
    //}

    public virtual void Draw()
    {
        var textField = new TextField()
        {
            value = NodeName,
        };

        titleContainer.Insert(0, textField);
        var inputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(bool));

        inputContainer.Add(inputPort);
    }

    public void DisconnectAllPorts()
    {
        DisconnectInputPorts();
        DisconnectOutputPorts();
    }

    public void DisconnectInputPorts()=> DisconnectPorts(inputContainer);

    public void DisconnectOutputPorts() => DisconnectPorts(outputContainer);

    private void DisconnectPorts(VisualElement container)
    {
        foreach (Port port in container.Children())
            if (port.connected)
                m_GraphView.DeleteElements(port.connections);
    }
}
