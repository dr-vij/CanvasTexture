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

        data.Init(1024);
        data.ClearWithColor(Color.blue);
        data.DrawLine(new float2(-1, -1), new float2(1, 1), 0.1f, Color.white);
        data.DrawLine(new float2(-1, 1), new float2(1, -1), 0.1f, Color.white);
        data.Flush();

        m_Renderer.material.mainTexture = data.ToTexture2D();
    }
}
