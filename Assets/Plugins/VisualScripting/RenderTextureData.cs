using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public partial class RenderTextureData
{
    public CommandBuffer CommandBuffer { get; private set; }
    public RenderTexture RenderTexture { get; private set; }

    private RenderTargetIdentifier mRt;

    public void Init(RenderTextureDescriptor settings)
    {
        CommandBuffer = new CommandBuffer();
        var id = Shader.PropertyToID("ColorBuffer");
        mRt = new RenderTargetIdentifier(id);

        RenderTexture = new RenderTexture(settings);

        CommandBuffer.GetTemporaryRT(id, settings);
        CommandBuffer.SetRenderTarget(mRt);
    }

    public void Flush()
    {
        CommandBuffer.CopyTexture(mRt, RenderTexture);
        Graphics.ExecuteCommandBuffer(CommandBuffer);
        CommandBuffer.Clear();
        SaveRenderTextureToFile("Assets/screenshot.png");
    }

    public void SaveRenderTextureToFile(string path)
    {
        var texture2d = new Texture2D(RenderTexture.width, RenderTexture.height);
        var act = RenderTexture.active;
        RenderTexture.active = RenderTexture;
        texture2d.ReadPixels(new Rect(0, 0, RenderTexture.width, RenderTexture.height), 0, 0);
        texture2d.Apply();
        var bytes = texture2d.EncodeToPNG();
        System.IO.File.WriteAllBytes(path, bytes);
    }
}