using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

[Widget(typeof(RenderTextureClearNode))]
public sealed class RenderTextureClearNodeWidget : UnitWidget<RenderTextureClearNode>
{
    private EditorTexture valueIcon;
    private bool mouseIsOver;

    public override bool foregroundRequiresInput => true;

    public RenderTextureClearNodeWidget(FlowCanvas canvas, RenderTextureClearNode unit) : base(canvas, unit)
    {
    }

    public override void DrawForeground()
    {
        base.DrawForeground();
        //var inputHasConnection = inputs[0].port.hasAnyConnection;
        //var outputHasConnection = outputs[0].port.hasAnyConnection;
        //mouseIsOver = new Rect(_position.x - 20, _position.y - 10, mouseIsOver ? 80 : 40, 40).Contains(mousePosition);

        //if (isSelected || mouseIsOver || !inputHasConnection || !outputHasConnection)
        //{
        //    _position.width = 26;
        //    GraphGUI.Node(new Rect(position.x, position.y + 3, 26, _position.height - 4), NodeShape.Square, NodeColor.Gray, isSelected);
        //}
        //else
        //{
        //    _position.width = -19;
        //}
        //Reposition();
    }

    public override void CachePosition()
    {
        base.CachePosition();
        //var inputPort = inputs[0].port;
        //var outputPort = outputs[0].port;
        //var inputHasConnection = inputPort.hasAnyConnection;
        //var outputHasConnection = outputPort.hasAnyConnection;
        //_position.x = unit.position.x;
        //_position.y = unit.position.y;

        //_position.width = !inputHasConnection || !outputHasConnection || isSelected || mouseIsOver ? 26 : -19;

        //_position.height = 20;

        //inputs[0].y = _position.y + 5;
        //outputs[0].y = _position.y + 5;

        //if (valueIcon == null && (inputPort.Descriptor()).description.icon != null) valueIcon = ((UnitPortDescriptor)inputPort.Descriptor()).description.icon;

        //if (inputHasConnection && !outputHasConnection) { ((UnitPortDescriptor)inputPort.Descriptor()).description.icon = null; }
        //((UnitPortDescriptor)inputPort.Descriptor()).description.icon = !inputHasConnection || isSelected || mouseIsOver ? valueIcon : null;
        //((UnitPortDescriptor)outputPort.Descriptor()).description.icon = !outputHasConnection || isSelected || mouseIsOver ? valueIcon : null;
    }
}