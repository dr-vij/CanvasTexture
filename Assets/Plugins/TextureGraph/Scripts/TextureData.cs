using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using ViJApps.MathematicsExtensions;

namespace ViJApps
{
    public enum ShaderType
    {
        None = 0,
        SimpleUnlit = 1,
    }

    public partial class TextureData : IDisposable
    {
        protected CommandBuffer m_Cmd;
        protected RenderTexture m_RTex;
        protected RenderTextureDescriptor m_Desc;

        protected List<Mesh> m_AllocatedMeshes = new List<Mesh>();
        protected List<Material> m_AllocatedMaterials = new List<Material>();

        private float2 m_TextureSize = float2.zero;

        public RenderTexture RTex => m_RTex;

        public float Aspect { get; set; } = 1f;

        public void Init(RenderTexture rtex)
        {
            ReinitCMD();

            m_Desc = rtex.descriptor;
            m_RTex = rtex;
        }

        public void Init(int size) => Init(size, size);

        public void Init(int width, int height)
        {
            ReinitCMD();
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
            //We convert from 0..textureSize to -1..1 here
            var scaleMatrix = MathUtils2d.CreateTranslationScaleMatrix(m_TextureSize / 2, m_TextureSize / 2);

            var mesh = MeshTools.CreateLine(pixelFrom, pixelTo, scaleMatrix, pixelThickness, Pooler.Instance.GetMesh());

            var m_Mat = MaterialProvider.Instance.GetMaterial(ShaderType.SimpleUnlit);
            var propertyBlock = Pooler.Instance.GetPropertyBlock();
            propertyBlock.SetColor(MaterialProvider.Instance.ColorPropertyID, color);
            m_Cmd.DrawMesh(mesh, Matrix4x4.identity, m_Mat, 0, -1, propertyBlock);
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

        private void ClearPools()
        {
        }

        private void ReinitCMD()
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