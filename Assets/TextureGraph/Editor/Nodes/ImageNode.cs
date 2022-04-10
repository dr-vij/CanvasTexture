using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class ImageData
{
}

public class ImageNode : TextureGraphNode
{
    public Vector2Int Size { get; set; }

    public ImageNode(Vector2 position, GraphView view) :  base(position, view)
    {
        Size = new Vector2Int(512, 512);
    }

    protected override void InitExtensionContainer()
    {
        base.InitExtensionContainer();
        var sizeField = new Vector2IntField("ImageSize");
        extensionContainer.Add(sizeField);
    }

    protected override void InitPorts()
    {
        base.InitPorts();
        var inputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(ImageData));
        inputContainer.Add(inputPort);
    }
}
