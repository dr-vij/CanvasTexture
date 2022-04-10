using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using System;

[Serializable]
public class ColorNodeData : NodeSaveData
{
}

public class ColorNode : DataNode<ColorNodeData>
{
    public Color Color { get; set; }

    public override NodeTypes NodeType => NodeTypes.ColorNode;

    public ColorNode(GraphView view) : base(view)
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
