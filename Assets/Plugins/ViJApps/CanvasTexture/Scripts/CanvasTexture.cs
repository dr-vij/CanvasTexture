using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using ViJApps.CanvasTexture.Utils;
using MathUtils = ViJApps.CanvasTexture.Utils.MathUtils;

namespace ViJApps.CanvasTexture
{
    public enum SimpleLineEndingStyle
    {
        None = 0,
        Round = 1,
    }

    public partial class CanvasTexture : IDisposable
    {
        //Data
        private CommandBuffer m_cmd;
        private RenderTextureDescriptor m_textureDescriptor;

        //Pools
        private readonly MeshPool m_meshPool = new();
        private readonly List<Mesh> m_allocatedMeshes = new();
        private readonly PropertyBlockPool m_propertyBlockPool = new();
        private readonly List<MaterialPropertyBlock> m_allocatedPropertyBlocks = new();
        private readonly TextComponentsPool m_textComponentsPool = new();
        private readonly List<TextComponent> m_allocatedTextComponents = new();

        //coord systems used for painting
        private LinearCoordSystem m_textureCoordSystem;

        public readonly AspectSettings AspectSettings = new();

        public RenderTexture RenderTexture { get; private set; }

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

        public void DrawComplexPolygon(
            List<List<float2>> solidPolygons,
            List<List<float2>> holesPolygons,
            float lineThickness,
            Color fillColor,
            Color lineColor,
            float lineOffset = 0.5f,
            LineJoinType joinType = LineJoinType.Miter,
            float miterLimit = 0f)
        {
            var (fillMesh, fillBlock) = AllocateMeshAndPropertyBlock();
            var (lineMesh, lineBlock) = AllocateMeshAndPropertyBlock();

            //Transform points with aspect matrix
            var solidPolygonsTransformed = solidPolygons.TransformPoints(AspectSettings.InverseAspectMatrix2d);
            var holesPolygonsTransformed = holesPolygons.TransformPoints(AspectSettings.InverseAspectMatrix2d);
            //transform points from solidPolygons to solidPolygonsTransformed

            MeshTools.CreatePolygon(
                solidPolygonsTransformed,
                holesPolygonsTransformed,
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

            m_cmd.DrawMesh(fillMesh, AspectSettings.AspectMatrix3d, fillMaterial, 0, 0, fillBlock);
            m_cmd.DrawMesh(lineMesh, AspectSettings.AspectMatrix3d, lineMaterial, 0, 0, lineBlock);
        }

        /// <summary>
        /// Draw complex polygon
        /// </summary>
        /// <param name="solidPolygons"></param>
        /// <param name="lineThickness"></param>
        /// <param name="fillColor"></param>
        /// <param name="lineColor"></param>
        /// <param name="lineOffset"></param>
        /// <param name="joinType"></param>
        /// <param name="miterLimit"></param>
        public void DrawComplexPolygon(
            List<List<float2>> solidPolygons,
            float lineThickness,
            Color fillColor,
            Color lineColor,
            float lineOffset = 0.5f,
            LineJoinType joinType = LineJoinType.Miter,
            float miterLimit = 0f)
            => DrawComplexPolygon(
                solidPolygons,
                new List<List<float2>>(),
                lineThickness,
                fillColor,
                lineColor,
                lineOffset,
                joinType,
                miterLimit);

        public void DrawSimplePolygon(List<List<float2>> contours, Color color)
        {
            var (mesh, propertyBlock) = AllocateMeshAndPropertyBlock();
            mesh = MeshTools.CreateMeshFromContourPolygons(contours, mesh);

            var material = MaterialProvider.GetMaterial(MaterialProvider.SimpleUnlitShaderId);
            propertyBlock.SetColor(MaterialProvider.ColorPropertyId, color);
            m_cmd.DrawMesh(mesh, Matrix4x4.identity, material, 0, 0, propertyBlock);
        }

        //Draw ellipse
        public void DrawEllipsePixels(float2 pixelsCenter, float2 radiusAbPixels, Color color)
        {
            //Convert to texture coordinates
            var center = pixelsCenter.TransformPoint(m_textureCoordSystem.WorldToZeroOne2d);
            var ab = radiusAbPixels.TransformDirection(m_textureCoordSystem.WorldToZeroOne2d);

            //Draw in texture coordinates
            DrawEllipsePercent(center, ab, 0f, color, color);
        }

        public void DrawEllipsePercent(float2 center, float2 radiusAb, float strokeWidth, Color fillColor, Color strokeColor, float strokeOffset = 0.5f)
        {
            var (mesh, propertyBlock) = AllocateMeshAndPropertyBlock();
            var transform = AspectSettings.AspectMatrix2d;

            //We create rect for this ellipse, so we use 2x radius and 2x strokeWidth
            var (positiveOffset, negativeOffset) = GetOffsets(strokeOffset, strokeWidth);
            var innerPart = math.max(radiusAb + new float2(negativeOffset, negativeOffset), float2.zero) ;
            var outerPart = math.max(radiusAb + new float2(positiveOffset, positiveOffset), float2.zero)  ;

            mesh = MeshTools.CreateRectTransformed(outerPart * 2, transform, mesh);

            //Prepare property block parameters for ellipse
            propertyBlock.SetColor(MaterialProvider.FillColorPropertyId, fillColor);
            propertyBlock.SetColor(MaterialProvider.StrokeColorPropertyId, strokeColor);
            propertyBlock.SetVector(MaterialProvider.AbFillStrokePropertyId, new Vector4(innerPart.x, innerPart.y, outerPart.x, outerPart.y));

            var inverseTransform = math.inverse(transform);
            propertyBlock.SetMatrix(MaterialProvider.TransformMatrixPropertyId,
                new Matrix4x4(inverseTransform.c0.XYZ0(), inverseTransform.c1.XYZ0(), inverseTransform.c2.XYZ0(), new float4(0, 0, 0, 1)));

            //Ellipse material
            var material = MaterialProvider.GetMaterial(MaterialProvider.SimpleEllipseUnlitShaderId);

            //Draw with offset by center
            m_cmd.DrawMesh(mesh, MathUtils.CreateMatrix3d_T(new float3(center, 0)), material, 0, -1, propertyBlock);
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

            var rectMesh = MeshTools.CreateRect(center, size, AspectSettings.AspectMatrix2d, mesh);
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

            propertyBlock.SetFloat(MaterialProvider.AspectPropertyId, AspectSettings.Aspect);

            var circleMesh = MeshTools.CreateRect(center, new float2(radius, radius), AspectSettings.AspectMatrix2d, mesh);
            var lineMaterial = MaterialProvider.GetMaterial(MaterialProvider.SimpleCircleUnlitShaderId);

            m_cmd.DrawMesh(circleMesh, Matrix4x4.identity, lineMaterial, 0, -1, propertyBlock);
        }

        //Draw line
        public void DrawLinePixels(float2 pixelFromCoord, float2 pixelToCoord, float pixelThickness, Color color, SimpleLineEndingStyle endingStyle = SimpleLineEndingStyle.None)
        {
            var texFromCoord = pixelFromCoord.TransformPoint(m_textureCoordSystem.WorldToZeroOne2d);
            var texToCoord = pixelToCoord.TransformPoint(m_textureCoordSystem.WorldToZeroOne2d);
            var thickness = pixelThickness / m_textureCoordSystem.Height;

            DrawLinePercent(texFromCoord, texToCoord, thickness, color, endingStyle);
        }

        public void DrawText(string text, TextSettings textSettings, float2 position, float rotation = 0)
            => DrawText(text, textSettings, position, sizeDelta: new float2(1, 1), rotation: rotation);

        public void DrawText(string text, TextSettings textSettings, float2 position, float2 sizeDelta, float rotation = 0)
            => DrawText(text, textSettings, position, sizeDelta, rotation, pivot: new float2(0.5f, 0.5f));

        public void DrawText(string text, TextSettings textSettings, float2 position, float2 sizeDelta, float rotation, float2 pivot)
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
            textComponent.Aspect = AspectSettings.Aspect;

            //Set settings, update mesh and add to render
            textComponent.SetSettings(textSettings);
            textComponent.UpdateText();
            m_cmd.DrawRenderer(textComponent.Renderer, textComponent.Material);
        }

        public void DrawLinePercent(float2 percentFromCoord, float2 percentToCoord, float percentHeightThickness, Color color, SimpleLineEndingStyle endingStyle = SimpleLineEndingStyle.None)
        {
            var (lineMesh, propertyBlock) = AllocateMeshAndPropertyBlock();

            propertyBlock.SetColor(MaterialProvider.ColorPropertyId, color);
            Material lineMaterial;
            switch (endingStyle)
            {
                case SimpleLineEndingStyle.None:
                    lineMesh = MeshTools.CreateLine(percentFromCoord, percentToCoord, AspectSettings.AspectMatrix2d,
                        percentHeightThickness, false,
                        lineMesh);
                    lineMaterial =
                        MaterialProvider.GetMaterial(MaterialProvider.SimpleUnlitShaderId);
                    break;
                case SimpleLineEndingStyle.Round:
                    lineMesh = MeshTools.CreateLine(percentFromCoord, percentToCoord, AspectSettings.AspectMatrix2d,
                        percentHeightThickness, true,
                        lineMesh);
                    lineMaterial =
                        MaterialProvider.GetMaterial(MaterialProvider.SimpleLineUnlitShaderId);
                    propertyBlock.SetFloat(MaterialProvider.AspectPropertyId, AspectSettings.Aspect);
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

        public void Dispose()
        {
            ReleaseToPools();
            m_meshPool.Clear();
            m_propertyBlockPool.Clear();
            m_textComponentsPool.Clear();

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

        private (Mesh mesh, MaterialPropertyBlock block) AllocateMeshAndPropertyBlock()
        {
            var mesh = m_meshPool.Get();
            m_allocatedMeshes.Add(mesh);

            var propertyBlock = m_propertyBlockPool.Get();
            m_allocatedPropertyBlocks.Add(propertyBlock);
            return (mesh, propertyBlock);
        }

        private (float positiveOffset, float negativeOffset) GetOffsets(float strokeOffset, float strokeThickness)
        {
            strokeOffset = math.clamp(strokeOffset, 0, 1);
            return (strokeThickness * strokeOffset, -strokeThickness * (1f - strokeOffset));
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
    }
}