using System;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using ViJApps.MathematicsExtensions;

namespace ViJApps
{
    public class MeshPool : ObjectPool<Mesh>
    {
        public MeshPool() : base((c) => c = new Mesh(), (c) => c.Clear(), true) { }
    }

    public partial class TextureData : IDisposable
    {
        protected static MeshPool m_MeshPool = new MeshPool();

        protected CommandBuffer m_Cmd;
        protected RenderTexture m_RTex;
        protected RenderTextureDescriptor m_Desc;

        private float2 m_TextureSize = float2.zero;

        public RenderTexture RTex => m_RTex;

        public float Aspect { get; set; } = 1f;

        public void Init(RenderTexture rtex)
        {
            InitCMD();

            m_Desc = rtex.descriptor;
            m_RTex = rtex;
        }

        public void Init(int size) => Init(size, size);

        public void Init(int width, int height)
        {
            InitCMD();
            if (m_RTex == null || m_RTex.width != width || m_RTex.height != height)
                ReinitTexture(width, height);
        }

        public void Flush()
        {
            Graphics.ExecuteCommandBuffer(m_Cmd);
            m_Cmd.Clear();
        }

        public void ClearWithColor(Color color)
        {
            m_Cmd.Clear();
            m_Cmd.SetRenderTarget(m_RTex);
            m_Cmd.ClearRenderTarget(RTClearFlags.All, color, 1f, 0);
        }

        public void DrawLinePixels(float2 pixelFrom, float2 pixelTo, float pixelThickness, Color color)
        {
            //We convert from 0..1 to -1..1 here
            var scaleMatrix = Utils.CreateTranslationScaleMatrix(m_TextureSize/2, m_TextureSize/2);

            var mesh = MeshTools.CreateLine(pixelFrom, pixelTo, scaleMatrix, pixelThickness, m_MeshPool.Get());

            var m_Mat = new Material(Shader.Find("Unlit/Color"));
            m_Mat.color = color;

            m_Cmd.DrawMesh(mesh, Matrix4x4.identity, m_Mat);
        }

        public Texture2D ToTexture2D(Texture2D texture = null)
        {
            if (texture == null)
                texture = new Texture2D(m_RTex.width, m_RTex.height);
            else
                texture.Reinitialize(m_RTex.width, m_RTex.height);

            var buffer = RenderTexture.active;
            RenderTexture.active = RTex;
            texture.ReadPixels(new Rect(0, 0, m_RTex.width, m_RTex.height), 0, 0);
            texture.Apply();
            RenderTexture.active = buffer;
            return texture;
        }

        public void SaveToAssets(string assetName)
        {
#if UNITY_EDITOR
            var texture2d = ToTexture2D();
            var bytes = texture2d.EncodeToPNG();
            System.IO.File.WriteAllBytes($"Assets/" + assetName + ".png", bytes);
            AssetDatabase.Refresh();
#else
            Debug.LogWarning("You can save to assets only from Editor");
#endif
        }

        private void InitCMD()
        {
            if (m_Cmd == null)
                m_Cmd = new CommandBuffer();
            else
                m_Cmd.Clear();
        }

        private void ReinitTexture(int width, int height)
        {
            m_Desc = new RenderTextureDescriptor(width, height);
            m_TextureSize = new float2(width, height);
            if (m_RTex != null)
                UnityEngine.Object.Destroy(m_RTex);
            m_RTex = new RenderTexture(m_Desc);
        }

        public void Dispose()
        {
            if (m_Cmd != null)
            {
                m_Cmd.Dispose();
                m_Cmd = null;
            }
            if (m_RTex != null)
            {
                UnityEngine.Object.Destroy(m_RTex);
                m_RTex = null;
            }
        }
    }
}