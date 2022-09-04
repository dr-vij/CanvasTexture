using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderTextureClearNode : RenderTextureBaseNode
{
    protected override void Paint(RenderTextureData data)
    {
        data.CommandBuffer.ClearRenderTarget(true, true, Color.green);
    }
}
