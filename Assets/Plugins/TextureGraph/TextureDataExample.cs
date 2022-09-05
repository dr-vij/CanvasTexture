using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ViJApps;

public class TextureDataExample : MonoBehaviour
{
    [SerializeField] private Renderer m_Renderer1 = default;
    [SerializeField] private Renderer m_Renderer2 = default;

    private void Start()
    {
        var data = new TextureData();

        data.Init(1024);
        data.ClearWithColor(Color.blue);
        data.DrawLineV1(Vector2.zero, Vector2.zero, 1f, Color.white);
        data.Flush();
        m_Renderer1.material.mainTexture = data.ToTexture2d();

        data = new TextureData();

        data.Init(1024);
        data.ClearWithColor(Color.blue);
        data.DrawLineV2(Vector2.zero, Vector2.zero, 1f, Color.white);
        data.Flush();
        m_Renderer2.material.mainTexture = data.ToTexture2d();
    }
}
