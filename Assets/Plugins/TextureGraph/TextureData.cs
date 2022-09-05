using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace ViJApps
{
    public partial class TextureData : IDisposable
    {
        protected CommandBuffer m_Cmd;
        protected RenderTexture m_RTex;
        protected RenderTextureDescriptor m_Desc;

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

        public void DrawLineV1(Vector2 from, Vector2 to, float width, Color color)
        {
            var mesh = new Mesh();
            mesh.SetVertices(new Vector3[] { Vector3.one * 0, Vector3.up * 0.5f, Vector3.one * 0.5f, Vector3.right * 0.5f });
            mesh.SetIndices(new int[] { 0, 1, 2, 0, 2, 3 }, MeshTopology.Triangles, 0);

            var matrix = GL.GetGPUProjectionMatrix(Matrix4x4.identity, true);
            m_Cmd.DrawMesh(mesh, matrix, new Material(Shader.Find("Standard")));
        }

        public void DrawLineV2(Vector2 from, Vector2 to, float width, Color color)
        {
            var mesh = new Mesh();
            mesh.SetVertices(new Vector3[] { Vector3.one * 0, Vector3.up * 0.5f, Vector3.one * 0.5f, Vector3.right * 0.5f });
            mesh.SetIndices(new int[] { 0, 1, 2, 0, 2, 3 }, MeshTopology.Triangles, 0);

            var matrix = Matrix4x4.identity;
            m_Cmd.DrawMesh(mesh, matrix, new Material(Shader.Find("Standard")));
        }

        public Texture2D ToTexture2d()
        {
            var texture2d = new Texture2D(m_RTex.width, m_RTex.height);
            var buffer = RenderTexture.active;
            RenderTexture.active = RTex;
            texture2d.ReadPixels(new Rect(0, 0, m_RTex.width, m_RTex.height), 0, 0);
            texture2d.Apply();
            RenderTexture.active = buffer;
            return texture2d;
        }

        public void SaveToAssets(string assetName)
        {
            var texture2d = ToTexture2d();
            var bytes = texture2d.EncodeToPNG();
            System.IO.File.WriteAllBytes($"Assets/" + assetName + ".png", bytes);
            AssetDatabase.Refresh();
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