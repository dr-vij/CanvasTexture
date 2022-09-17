using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using ViJApps.TextureGraph.Utils;

namespace ViJApps.TextureGraph
{
    public enum SimpleLineEndingStyle
    {
        None = 0,
        Round = 1,
    }

    public class TextureData : IDisposable
    {
        //Data
        private CommandBuffer m_cmd;
        private RenderTextureDescriptor m_textureDescriptor;

        //Pool parts
        private readonly MeshPool m_meshPool = new();
        private readonly List<Mesh> m_allocatedMeshes = new();
        private readonly PropertyBlockPool m_propertyBlockPool = new();
        private readonly List<MaterialPropertyBlock> m_allocatedPropertyBlocks = new();

        private LinearCoordSystem m_textureCoordSystem;

        public RenderTexture RenderTexture { get; private set; }

        private float3x3 m_aspectMatrix = float3x3.identity;
        private float3x3 m_inverseAspectMatrix = float3x3.identity;
        private float m_aspect = 1f;

        /// <summary>
        /// Aspect interpretation
        /// </summary>
        public float Aspect
        {
            get => m_aspect;
            set
            {
                m_aspect = value;
                m_aspectMatrix = Utils.MathUtils.CreateMatrix2d_S(new float2(m_aspect, 1));
                m_inverseAspectMatrix = math.inverse(m_aspectMatrix);
            }
        }

        public void Init(RenderTexture renderTexture)
        {
            ResetCmd();
            ReleaseToPools();

            m_textureDescriptor = renderTexture.descriptor;
            RenderTexture = renderTexture;
        }

        public void Init(int size) => Init(size, size);

        public void Init(int width, int height)
        {
            ResetCmd();
            ReleaseToPools();

            if (RenderTexture == null || RenderTexture.width != width || RenderTexture.height != height)
                ReinitTexture(width, height);
        }

        public void Flush()
        {
            Graphics.ExecuteCommandBuffer(m_cmd);

            ResetCmd();
            ReleaseToPools();
        }

        public void ClearWithColor(Color color)
        {
            ResetCmd();
            ReleaseToPools();

            m_cmd.SetRenderTarget(RenderTexture);
            m_cmd.ClearRenderTarget(RTClearFlags.All, color, 1f, 0);
        }

        public void DrawLinePercent(float2 percentFromCoord, float2 percentToCoord, float percentHeightThickness,
            Color color, SimpleLineEndingStyle endingStyle = SimpleLineEndingStyle.None)
        {
            Material lineMaterial;
            var lineMesh = m_meshPool.Get();
            m_allocatedMeshes.Add(lineMesh);

            var propertyBlock = m_propertyBlockPool.Get();
            m_allocatedPropertyBlocks.Add(propertyBlock);

            propertyBlock.SetColor(MaterialProvider.Instance.Color_PropertyID, color);

            switch (endingStyle)
            {
                case SimpleLineEndingStyle.None:
                    lineMesh = MeshTools.CreateLine(percentFromCoord, percentToCoord, m_aspectMatrix,
                        percentHeightThickness, false,
                        lineMesh);
                    lineMaterial =
                        MaterialProvider.Instance.GetMaterial(MaterialProvider.Instance.SimpleUnlit_ShaderID);
                    break;
                case SimpleLineEndingStyle.Round:
                    lineMesh = MeshTools.CreateLine(percentFromCoord, percentToCoord, m_aspectMatrix,
                        percentHeightThickness, true,
                        lineMesh);
                    lineMaterial =
                        MaterialProvider.Instance.GetMaterial(MaterialProvider.Instance.SimpleLineUnlit_ShaderID);
                    propertyBlock.SetFloat(MaterialProvider.Instance.Aspect_PropertyID, Aspect);
                    propertyBlock.SetFloat(MaterialProvider.Instance.Thickness_PropertyID, percentHeightThickness);
                    propertyBlock.SetVector(MaterialProvider.Instance.FromToCoord_PropertyID,
                        new Vector4(percentFromCoord.x, percentFromCoord.y, percentToCoord.x, percentToCoord.y));
                    break;
                default:
                    throw new Exception("Unknown line ending style");
            }

            m_cmd.DrawMesh(lineMesh, Matrix4x4.identity, lineMaterial, 0, -1, propertyBlock);
        }

        public void DrawLinePixels(float2 pixelFromCoord, float2 pixelToCoord, float pixelThickness, Color color,
            SimpleLineEndingStyle endingStyle = SimpleLineEndingStyle.None)
        {
            var texFromCoord = pixelFromCoord.TransformPoint(m_textureCoordSystem.WorldToZeroOne2d);
            var texToCoord = pixelToCoord.TransformPoint(m_textureCoordSystem.WorldToZeroOne2d);
            var thickness = pixelThickness / m_textureCoordSystem.Height;

            DrawLinePercent(texFromCoord, texToCoord, thickness, color, endingStyle);
        }

        public Texture2D ToTexture2D(Texture2D texture = null)
        {
            if (texture == null)
                texture = new Texture2D(RenderTexture.width, RenderTexture.height);
            else
                texture.Reinitialize(RenderTexture.width, RenderTexture.height);

            var buffer = RenderTexture.active;
            RenderTexture.active = RenderTexture;
            texture.ReadPixels(new Rect(0, 0, RenderTexture.width, RenderTexture.height), 0, 0);
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

        private void ResetCmd()
        {
            if (m_cmd == null)
                m_cmd = new CommandBuffer();
            else
                m_cmd.Clear();
            m_cmd.SetViewProjectionMatrices(Utils.MathUtils.Mtr3dZeroOneToMinusOnePlusOne, Matrix4x4.identity);
        }

        private void ReinitTexture(int width, int height)
        {
            m_textureDescriptor = new RenderTextureDescriptor(width, height);
            if (RenderTexture != null)
                UnityEngine.Object.Destroy(RenderTexture);
            m_textureCoordSystem = new LinearCoordSystem(new float2(width, height));
            RenderTexture = new RenderTexture(m_textureDescriptor);
        }

        private void ReleaseToPools()
        {
            foreach (var mesh in m_allocatedMeshes)
                m_meshPool.Release(mesh);
            m_allocatedMeshes.Clear();

            foreach (var block in m_allocatedPropertyBlocks)
                m_propertyBlockPool.Release(block);
            m_allocatedPropertyBlocks.Clear();
        }

        public void Dispose()
        {
            ReleaseToPools();
            m_meshPool.Clear();
            m_propertyBlockPool.Clear();

            if (m_cmd != null)
            {
                m_cmd.Dispose();
                m_cmd = null;
            }

            if (RenderTexture != null)
            {
                UnityEngine.Object.Destroy(RenderTexture);
                RenderTexture = null;
            }
        }
    }
}