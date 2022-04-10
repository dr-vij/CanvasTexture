using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;

public class ColorNode : TextureGraphNode
{
    public Color Color { get; set; }

    public ColorNode(Vector2 position, GraphView view) : base(position, view)
    {
    }

    public override void Draw()
    {
        var outputPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(Color));
        outputContainer.Add(outputPort);

        var colorField = new ColorField("Color");
        extensionContainer.Add(colorField);
        RefreshExpandedState();
    }
}
