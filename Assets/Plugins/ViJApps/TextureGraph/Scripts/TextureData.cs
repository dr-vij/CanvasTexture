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

    public partial class TextureData : IDisposable
    {
        //Data
        private CommandBuffer m_cmd;
        private RenderTextureDescriptor m_textureDescriptor;

        //Pool parts
        private readonly MeshPool m_meshPool = new();
        private readonly List<Mesh> m_allocatedMeshes = new();
        private readonly PropertyBlockPool m_propertyBlockPool = new();
        private readonly List<MaterialPropertyBlock> m_allocatedPropertyBlocks = new();
        private readonly TextComponentsPool m_textComponentsPool = new();
        private readonly List<TextComponent> m_allocatedTextComponents = new();

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
                m_aspect = math.max(value, 0.0001f);
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

        //Draw polygon
        public void DrawComplexPolygon(
            List<List<float2>> solidPolygons,
            float lineThickness,
            Color fillColor,
            Color lineColor,
            float lineOffset = 0.5f,
            LineJoinType joinType = LineJoinType.Miter,
            float miterLimit = 0f)
        {
            var (fillMesh, fillBlock) = AllocateMeshAndPropertyBlock();
            var (lineMesh, lineBlock) = AllocateMeshAndPropertyBlock();

            MeshTools.CreatePolygon(
                solidPolygons,
                lineThickness,
                lineOffset,
                joinType,
                miterLimit,
                fillMesh,
                lineMesh);

            var fillMaterial = MaterialProvider.GetMaterial(MaterialProvider.SimpleUnlitTransparentShaderId);
            var lineMaterial = MaterialProvider.GetMaterial(MaterialProvider.SimpleUnlitTransparentShaderId);
            
            fillBlock.SetColor(MaterialProvider.ColorPropertyId, fillColor);
            lineBlock.SetColor(MaterialProvider.ColorPropertyId, lineColor);

            m_cmd.DrawMesh(fillMesh, Matrix4x4.identity, fillMaterial, 0, 0, fillBlock);
            m_cmd.DrawMesh(lineMesh, Matrix4x4.identity, lineMaterial, 0, 0, lineBlock);
        }

        public void DrawSimplePolygon(List<List<float2>> contours, Color color)
        {
            var (mesh, propertyBlock) = AllocateMeshAndPropertyBlock();
            mesh = MeshTools.CreateMeshFromContourPolygons(contours, mesh);

            var material = MaterialProvider.GetMaterial(MaterialProvider.SimpleUnlitShaderId);
            propertyBlock.SetColor(MaterialProvider.ColorPropertyId, color);
            m_cmd.DrawMesh(mesh, Matrix4x4.identity, material, 0, 0, propertyBlock);
        }

        //Draw ellipse
        public void DrawEllipsePixels(float2 pixelsCenter, float2 abPixels, Color color)
        {
            //Convert to texture coordinates
            var center = pixelsCenter.TransformPoint(m_textureCoordSystem.WorldToZeroOne2d);
            var ab = pixelsCenter.TransformDirection(m_textureCoordSystem.WorldToZeroOne2d);

            //Draw in texture coordinates
            DrawEllipsePercent(center, ab, color);
        }

        public void DrawEllipsePercent(float2 center, float2 ab, Color color)
        {
            var (mesh, propertyBlock) = AllocateMeshAndPropertyBlock();
            //Prepare ellipse mesh. its a rectangle with 4 vertices / 2 triangles 
            mesh = MeshTools.CreateRect(center, ab * 2, m_aspectMatrix, mesh);

            //Prepare property block parameters for ellipse
            propertyBlock.SetColor(MaterialProvider.ColorPropertyId, color);
            propertyBlock.SetVector(MaterialProvider.AbPropertyId, new Vector4(ab.x, ab.y, 0, 0));
            propertyBlock.SetVector(MaterialProvider.CenterPropertyId, new Vector4(center.x, center.y, 0, 0));
            propertyBlock.SetFloat(MaterialProvider.AspectPropertyId, m_aspect);

            //Ellipse material
            var material = MaterialProvider.GetMaterial(MaterialProvider.SimpleEllipseUnlitShaderId);

            //Draw
            m_cmd.DrawMesh(mesh, Matrix4x4.identity, material, 0, -1, propertyBlock);
        }

        //Draw rect
        public void DrawRectPixels(float2 pixelsCenter, float2 pixelsSize, Color color)
        {
            var center = pixelsCenter.TransformPoint(m_textureCoordSystem.WorldToZeroOne2d);
            var size = pixelsSize.TransformDirection(m_textureCoordSystem.WorldToZeroOne2d);

            DrawRectPercent(center, size, color);
        }

        public void DrawRectPercent(float2 center, float2 size, Color color)
        {
            var (mesh, propertyBlock) = AllocateMeshAndPropertyBlock();

            var rectMesh = MeshTools.CreateRect(center, size, m_aspectMatrix, mesh);
            propertyBlock.SetColor(MaterialProvider.ColorPropertyId, color);

            var lineMaterial = MaterialProvider.GetMaterial(MaterialProvider.SimpleUnlitShaderId);
            m_cmd.DrawMesh(rectMesh, Matrix4x4.identity, lineMaterial, 0, -1, propertyBlock);
        }

        //Draw circle
        public void DrawCirclePixels(float2 pixelsCenter, float pixelsRadius, Color color)
        {
            var texCenter = pixelsCenter.TransformPoint(m_textureCoordSystem.WorldToZeroOne2d);
            var texRadius = pixelsRadius * m_textureCoordSystem.Height;

            DrawCirclePercent(texCenter, texRadius, color);
        }

        public void DrawCirclePercent(float2 center, float radius, Color color)
        {
            var (mesh, propertyBlock) = AllocateMeshAndPropertyBlock();

            propertyBlock.SetVector(MaterialProvider.CenterPropertyId, new Vector2(center.x, center.y));
            propertyBlock.SetFloat(MaterialProvider.RadiusPropertyId, radius);
            propertyBlock.SetColor(MaterialProvider.ColorPropertyId, color);

            propertyBlock.SetFloat(MaterialProvider.AspectPropertyId, Aspect);

            var circleMesh = MeshTools.CreateRect(center, new float2(radius, radius), m_aspectMatrix, mesh);
            var lineMaterial =
                MaterialProvider.GetMaterial(MaterialProvider.SimpleCircleUnlitShaderId);

            m_cmd.DrawMesh(circleMesh, Matrix4x4.identity, lineMaterial, 0, -1, propertyBlock);
        }

        //Draw line
        public void DrawLinePixels(float2 pixelFromCoord, float2 pixelToCoord, float pixelThickness, Color color,
            SimpleLineEndingStyle endingStyle = SimpleLineEndingStyle.None)
        {
            var texFromCoord = pixelFromCoord.TransformPoint(m_textureCoordSystem.WorldToZeroOne2d);
            var texToCoord = pixelToCoord.TransformPoint(m_textureCoordSystem.WorldToZeroOne2d);
            var thickness = pixelThickness / m_textureCoordSystem.Height;

            DrawLinePercent(texFromCoord, texToCoord, thickness, color, endingStyle);
        }

        public void DrawText(string text, TextSettings textSettings, float2 position, float rotation = 0)
            => DrawText(text, textSettings, position, sizeDelta: new float2(1, 1), rotation: rotation);

        public void DrawText(string text, TextSettings textSettings, float2 position, float2 sizeDelta,
            float rotation = 0)
            => DrawText(text, textSettings, position, sizeDelta, rotation, pivot: new float2(0.5f, 0.5f));

        public void DrawText(string text, TextSettings textSettings, float2 position, float2 sizeDelta, float rotation,
            float2 pivot)
        {
            //Prepare text mesh
            var textComponent = m_textComponentsPool.Get();
            m_allocatedTextComponents.Add(textComponent);
            textComponent.Text = text;

            //Position and size
            textComponent.Pivot = pivot;
            textComponent.Position = position;
            textComponent.SizeDelta = sizeDelta;
            textComponent.Rotation = rotation;
            textComponent.Aspect = Aspect;

            //Set settings, update mesh and add to render
            textComponent.SetSettings(textSettings);
            textComponent.UpdateText();
            m_cmd.DrawRenderer(textComponent.Renderer, textComponent.Material);
        }

        public void DrawLinePercent(float2 percentFromCoord, float2 percentToCoord, float percentHeightThickness,
            Color color, SimpleLineEndingStyle endingStyle = SimpleLineEndingStyle.None)
        {
            var (lineMesh, propertyBlock) = AllocateMeshAndPropertyBlock();

            propertyBlock.SetColor(MaterialProvider.ColorPropertyId, color);
            Material lineMaterial;
            switch (endingStyle)
            {
                case SimpleLineEndingStyle.None:
                    lineMesh = MeshTools.CreateLine(percentFromCoord, percentToCoord, m_aspectMatrix,
                        percentHeightThickness, false,
                        lineMesh);
                    lineMaterial =
                        MaterialProvider.GetMaterial(MaterialProvider.SimpleUnlitShaderId);
                    break;
                case SimpleLineEndingStyle.Round:
                    lineMesh = MeshTools.CreateLine(percentFromCoord, percentToCoord, m_aspectMatrix,
                        percentHeightThickness, true,
                        lineMesh);
                    lineMaterial =
                        MaterialProvider.GetMaterial(MaterialProvider.SimpleLineUnlitShaderId);
                    propertyBlock.SetFloat(MaterialProvider.AspectPropertyId, Aspect);
                    propertyBlock.SetFloat(MaterialProvider.ThicknessPropertyId, percentHeightThickness);
                    propertyBlock.SetVector(MaterialProvider.FromToCoordPropertyId,
                        new Vector4(percentFromCoord.x, percentFromCoord.y, percentToCoord.x, percentToCoord.y));
                    break;
                default:
                    throw new Exception("Unknown line ending style");
            }

            m_cmd.DrawMesh(lineMesh, Matrix4x4.identity, lineMaterial, 0, -1, propertyBlock);
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

        private (Mesh mesh, MaterialPropertyBlock block) AllocateMeshAndPropertyBlock()
        {
            var mesh = m_meshPool.Get();
            m_allocatedMeshes.Add(mesh);

            var propertyBlock = m_propertyBlockPool.Get();
            m_allocatedPropertyBlocks.Add(propertyBlock);
            return (mesh, propertyBlock);
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
            if (RenderTexture == null)
            {
                CreateRenderTexture(width, height);
            }
            else if (RenderTexture.width != width || RenderTexture.height != height)
            {
                UnityEngine.Object.Destroy(RenderTexture);
                CreateRenderTexture(width, height);
            }
        }

        private void CreateRenderTexture(int width, int height)
        {
            m_textureDescriptor = new RenderTextureDescriptor(width, height);
            RenderTexture = new RenderTexture(m_textureDescriptor);
            m_textureCoordSystem = new LinearCoordSystem(new float2(width, height));
        }

        private void ReleaseToPools()
        {
            foreach (var mesh in m_allocatedMeshes)
                m_meshPool.Release(mesh);
            m_allocatedMeshes.Clear();

            foreach (var block in m_allocatedPropertyBlocks)
                m_propertyBlockPool.Release(block);
            m_allocatedPropertyBlocks.Clear();

            foreach (var text in m_allocatedTextComponents)
                m_textComponentsPool.Release(text);
            m_allocatedTextComponents.Clear();
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