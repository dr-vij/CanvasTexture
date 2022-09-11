using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ViJApps;
using Unity.Mathematics;

public class TextureDataExample : MonoBehaviour
{
    [SerializeField] private Texture2D mTestTexture = default;
    [SerializeField] private Renderer mTestRenderer = default;

    private TextureData m_TextureData;

    private void Update()
    {
        m_TextureData = m_TextureData ?? new TextureData();

        var w = 1024;
        var h = 1024;
        var offset = 64f;
        var thickness = 64f;

        m_TextureData.Init(w, h);
        m_TextureData.Aspect = (float)w / h;
        m_TextureData.ClearWithColor(Color.blue);

        m_TextureData.DrawLinePixels(new float2(0 + offset, h - offset), new float2(w - offset, 0 + offset), thickness, Color.yellow, SimpleLineEndingStyle.Round);
        m_TextureData.DrawLinePixels(new float2(0 + offset, 0 + offset), new float2(w - offset, h - offset), thickness, Color.grey);
        m_TextureData.Flush();

        mTestTexture = m_TextureData.ToTexture2D(mTestTexture);
        mTestRenderer.material.mainTexture = mTestTexture;
    }
}
