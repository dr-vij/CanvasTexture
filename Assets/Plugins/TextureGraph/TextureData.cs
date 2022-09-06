using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;

namespace ViJApps
{
    public class MeshPool : ObjectPool<Mesh>
    {
        public MeshPool() : base((c) => new Mesh(), (c) => c.Clear(), true) { }
    }

    public partial class TextureData : IDisposable
    {
        protected CommandBuffer m_Cmd;
        protected RenderTexture m_RTex;
        protected static MeshPool m_MeshPool = new MeshPool();
        protected RenderTextureDescriptor m_Desc;

        protected float m_Aspect = 1f;

        public RenderTexture RTex => m_RTex;

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

        public void DrawLine(float2 from, float2 to, float width, Color color)
        {
            var scaleMat = new float2x2(new float2(1, 0), new float2(0, 8));

            var mesh = MeshTools.CreateLine(from, to, scaleMat, width, m_MeshPool.Get());
            var mat = new Material(Shader.Find("Unlit/Color"));
            mat.color = color;
            var mtrx = Matrix4x4.identity * Matrix4x4.Scale(new Vector3(1, 8f, 1));
            m_Cmd.DrawMesh(mesh, mtrx, mat);
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
            m_Aspect = (float)width / height;
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