using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using System;

public class ColorNode : TextureGraphNode
{
    public Color Color { get; set; }

    public ColorNode(Vector2 position, GraphView view) : base(position, view)
    {
    }

    protected override void InitPorts()
    {
        base.InitPorts();
        var outputPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(Color));
        outputContainer.Add(outputPort);
    }

    protected override void InitExtensionContainer()
    {
        base.InitExtensionContainer();
        var colorField = new ColorField("Color");
        extensionContainer.Add(colorField);
    }
}
