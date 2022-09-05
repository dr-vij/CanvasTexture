using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ViJApps;

public class TextureDataExample : MonoBehaviour
{
    private void Awake()
    {
        var data = new TextureData();
        data.Init(128, 128);
        data.Flush();
        data.SaveToAssets("Test");
    }
}
