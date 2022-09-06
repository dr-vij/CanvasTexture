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

        data.Init(2048, 256);
        data.ClearWithColor(Color.blue);
        data.DrawLine(new float2(-0.5f, -0.5f), new float2(0.5f, 0.5f), 0.2f, Color.grey);
        data.DrawLine(new float2(-0.5f, 0.5f), new float2(0.5f, -0.5f), 0.2f, Color.yellow);
        data.Flush();

        m_Renderer.material.mainTexture = data.ToTexture2D();
    }
}
