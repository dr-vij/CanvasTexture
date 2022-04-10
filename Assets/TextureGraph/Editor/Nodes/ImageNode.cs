using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

[Serializable]
public class ImageNodeData : NodeSaveData
{
    public Vector2Int ImageSize;
    public Color ClearColor;
}

public class ImageNode : DataNode<ImageNodeData>
{
    public Vector2Int ImageSize { get; set; }
    public Color ClearColor { get; set; }

    public override NodeTypes NodeType => NodeTypes.ImageNode;

    public ImageNode(GraphView view) : base(view)
    {
        ImageSize = new Vector2Int(512, 512);
    }

    protected override void InitExtensionContainer()
    {
        base.InitExtensionContainer();
        var sizeField = new Vector2IntField("ImageSize");
        var colorField = new ColorField("Clear Color");
        extensionContainer.Add(sizeField);
        extensionContainer.Add(colorField);
    }

    protected override void OnSaveData(ImageNodeData data)
    {
        base.OnSaveData(data);
        data.ClearColor = ClearColor;
        data.ImageSize = ImageSize;
    }

    protected override void OnApplyData(ImageNodeData data)
    {
        base.OnApplyData(data);
        ClearColor = data.ClearColor;
        ImageSize = data.ImageSize;
    }

    protected override void InitPorts()
    {
        base.InitPorts();

        //InPorts
        var inSizePort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(Vector2Int));
        var inColorPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(Color));
        AddInputPorts(inSizePort, inColorPort);

        //OutPorts
        var outImagePort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(ImageNodeData));
        AddOutputPorts(outImagePort);
    }
}
