using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using ViJApps.MathematicsExtensions;

namespace ViJApps
{
    public enum SimpleLineEndingStyle
    {
        None = 0,
        Round = 1,
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
            ResetCMD();
            m_Desc = rtex.descriptor;
            m_RTex = rtex;
        }

        public void Init(int size) => Init(size, size);

        public void Init(int width, int height)
        {
            ResetCMD();
            if (m_RTex == null || m_RTex.width != width || m_RTex.height != height)
                ReinitTexture(width, height);
        }

        public void Flush()
        {
            Graphics.ExecuteCommandBuffer(m_Cmd);
            ResetCMD();
        }

        public void ClearWithColor(Color color)
        {
            ResetCMD();
            m_Cmd.SetRenderTarget(m_RTex);
            m_Cmd.ClearRenderTarget(RTClearFlags.All, color, 1f, 0);
        }

        public void DrawLinePixels(float2 pixelFromCoord, float2 pixelToCoord, float pixelThickness, Color color, SimpleLineEndingStyle endingStyle = SimpleLineEndingStyle.None)
        {
            //We convert from 0..textureSize to -1..1 here
            var toTextureMatrix = MathUtils2d.CreateTranslationScaleMatrix(m_TextureSize / 2, m_TextureSize / 2);
            var inverseScaleMatrix = math.inverse(toTextureMatrix);

            var texFromCoord = MathUtils2d.TransformPoint(pixelFromCoord, inverseScaleMatrix);
            var texToCoord = MathUtils2d.TransformPoint(pixelToCoord, inverseScaleMatrix);

            Mesh lineMesh;
            Material lineMaterial;
            MaterialPropertyBlock propertyBlock = Pooler.Instance.GetPropertyBlock();
            propertyBlock.SetColor(MaterialProvider.Instance.ColorPropertyID, color);

            switch (endingStyle)
            {
                case SimpleLineEndingStyle.None:
                    lineMesh = MeshTools.CreateLine(pixelFromCoord, pixelToCoord, toTextureMatrix, pixelThickness, false, Pooler.Instance.GetMesh());
                    lineMaterial = MaterialProvider.Instance.GetMaterial(MaterialProvider.Instance.SimpleUnlitShaderID);
                    break;
                case SimpleLineEndingStyle.Round:
                    lineMesh = MeshTools.CreateLine(pixelFromCoord, pixelToCoord, toTextureMatrix, pixelThickness, true, Pooler.Instance.GetMesh());
                    lineMaterial = MaterialProvider.Instance.GetMaterial(MaterialProvider.Instance.SimpleLineUnlitShaderID);
                    propertyBlock.SetVector(MaterialProvider.Instance.FromToCoordID, new Vector4(texFromCoord.x, texFromCoord.y, texToCoord.x, texToCoord.y));
                    break;
                default:
                    throw new Exception("Unknown line ending style");
            }

            m_Cmd.DrawMesh(lineMesh, Matrix4x4.identity, lineMaterial, 0, -1, propertyBlock);
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

        private void ResetCMD()
        {
            if (m_Cmd == null)
                m_Cmd = new CommandBuffer();
            else
                m_Cmd.Clear();
            m_Cmd.SetViewProjectionMatrices(Matrix4x4.identity, Matrix4x4.identity);
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