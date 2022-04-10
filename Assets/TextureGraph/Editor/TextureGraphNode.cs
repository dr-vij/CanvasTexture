using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class TextureGraphNode : Node
{
    public string NodeName { get; set; }

    public Color Color { get; set; }

    public void Init(Vector2 position)
    {
        NodeName = "Texture Node";
        SetPosition(new Rect(position, Vector2.zero));
    }

    public void Draw()
    {
        var textField = new TextField()
        {
            value = NodeName,
        };

        titleContainer.Insert(0, textField);
        var inputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(bool));
        var outputPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(bool));

        inputContainer.Add(inputPort);
        outputContainer.Add(outputPort);

        var colorField = new ColorField("Color");
        extensionContainer.Add(colorField);
        RefreshExpandedState();
    }
}
