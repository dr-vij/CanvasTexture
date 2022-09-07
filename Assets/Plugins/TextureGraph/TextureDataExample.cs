using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ViJApps;
using Unity.Mathematics;

public class TextureDataExample : MonoBehaviour
{
    [SerializeField] private Renderer m_Renderer = default;

    private void Start()
    {
        var data = new TextureData();

        var w = 512;
        var h = 128;
        var offset = 32f;
        var thickness = 16f;

        data.Init(w, h);
        data.Aspect = (float)w / h;
        data.ClearWithColor(Color.blue);

        data.DrawLinePixels(new float2(0 + offset, 0 + offset), new float2(w - offset, h - offset), thickness, Color.grey);
        data.DrawLinePixels(new float2(0 + offset, h - offset), new float2(w - offset, 0 + offset), thickness, Color.yellow);
        data.Flush();

        m_Renderer.material.mainTexture = data.ToTexture2D();
    }
}
