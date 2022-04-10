using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class RenderNode : MonoBehaviour
{
    [SerializeField] private Camera m_Camera;

    private void Awake()
    {
        var cmd = new CommandBuffer();
        var identifier = new RenderTextureDescriptor(256, 256);
        RTHandle mTexture = RTHandles.Alloc(identifier);
        m_Camera.AddCommandBuffer(CameraEvent.AfterEverything, cmd);
        m_Camera.targetTexture = mTexture;
        cmd.ClearRenderTarget(true, true, Color.green);
        m_Camera.Render();
    }
}
