using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using ViJApps.Utils;

namespace ViJApps
{
    public enum SimpleLineEndingStyle
    {
        None = 0,
        Round = 1,
    }

    public class TextureData : IDisposable
    {
        //Data
        private CommandBuffer m_Cmd;
        private RenderTexture m_RTex;
        private RenderTextureDescriptor m_TexDesc;

        //Pool parts
        private MeshPool m_MeshPool = new MeshPool();
        private List<Mesh> m_AllocatedMeshes = new List<Mesh>();
        private PropertyBlockPool m_PropertyBlockPool = new PropertyBlockPool();
        private List<MaterialPropertyBlock> m_AllocatedPropertyBlocks = new List<MaterialPropertyBlock>();

        private float2 m_TexSize = float2.zero;

        public RenderTexture RTex => m_RTex;

        public float Aspect { get; set; } = 1f;

        public void Init(RenderTexture rtex)
        {
            ResetCMD();
            ReleaseToPools();

            m_TexDesc = rtex.descriptor;
            m_RTex = rtex;
        }

        public void Init(int size) => Init(size, size);

        public void Init(int width, int height)
        {
            ResetCMD();
            ReleaseToPools();

            if (m_RTex == null || m_RTex.width != width || m_RTex.height != height)
                ReinitTexture(width, height);
        }

        public void Flush()
        {
            Graphics.ExecuteCommandBuffer(m_Cmd);

            ResetCMD();
            ReleaseToPools();
        }

        public void ClearWithColor(Color color)
        {
            ResetCMD();
            ReleaseToPools();

            m_Cmd.SetRenderTarget(m_RTex);
            m_Cmd.ClearRenderTarget(RTClearFlags.All, color, 1f, 0);
        }

        public void DrawLinePixels(float2 pixelFromCoord, float2 pixelToCoord, float pixelThickness, Color color, SimpleLineEndingStyle endingStyle = SimpleLineEndingStyle.None)
        {
            var pixelSpaceToTexureSpaceMtrx = Utils.MathUtils.CreateMatrix3d_RemapToOneMinusOne(float2.zero, m_TexSize);
            var aspectScaleMatrix = Utils.MathUtils.CreateMatrix2d_S(new float2(Aspect, 1));

            var texFromCoord = Utils.MathUtils.TransformPoint(pixelFromCoord, pixelSpaceToTexureSpaceMtrx);
            var texToCoord = Utils.MathUtils.TransformPoint(pixelToCoord, pixelSpaceToTexureSpaceMtrx);
            var thickness = pixelThickness / m_TexSize.y;

            Material lineMaterial;

            var lineMesh = m_MeshPool.Get();
            m_AllocatedMeshes.Add(lineMesh);

            var propertyBlock = m_PropertyBlockPool.Get();
            m_AllocatedPropertyBlocks.Add(propertyBlock);

            propertyBlock.SetColor(MaterialProvider.Instance.Color_PropertyID, color);

            switch (endingStyle)
            {
                case SimpleLineEndingStyle.None:
                    lineMesh = MeshTools.CreateLine(texFromCoord, texToCoord, aspectScaleMatrix, thickness, false, lineMesh);
                    lineMaterial = MaterialProvider.Instance.GetMaterial(MaterialProvider.Instance.SimpleUnlit_ShaderID);
                    break;
                case SimpleLineEndingStyle.Round:
                    lineMesh = MeshTools.CreateLine(texFromCoord, texToCoord, aspectScaleMatrix, thickness, true, lineMesh);
                    lineMaterial = MaterialProvider.Instance.GetMaterial(MaterialProvider.Instance.SimpleLineUnlit_ShaderID);
                    propertyBlock.SetFloat(MaterialProvider.Instance.Aspect_PropertyID, Aspect);
                    propertyBlock.SetFloat(MaterialProvider.Instance.Thickness_PropertyID, thickness);
                    propertyBlock.SetVector(MaterialProvider.Instance.FromToCoord_PropertyID, new Vector4(texFromCoord.x, texFromCoord.y, texToCoord.x, texToCoord.y));
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
            m_TexDesc = new RenderTextureDescriptor(width, height);
            m_TexSize = new float2(width, height);
            if (m_RTex != null)
                UnityEngine.Object.Destroy(m_RTex);
            m_RTex = new RenderTexture(m_TexDesc);
        }

        private void ReleaseToPools()
        {
            foreach (var mesh in m_AllocatedMeshes)
                m_MeshPool.Release(mesh);
            m_AllocatedMeshes.Clear();

            foreach (var block in m_AllocatedPropertyBlocks)
                m_PropertyBlockPool.Release(block);
            m_AllocatedPropertyBlocks.Clear();
        }

        public void Dispose()
        {
            ReleaseToPools();
            m_MeshPool.Clear();
            m_PropertyBlockPool.Clear();

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