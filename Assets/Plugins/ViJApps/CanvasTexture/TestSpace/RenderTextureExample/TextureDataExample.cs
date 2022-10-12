using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using ViJApps.CanvasTexture.Utils;

namespace ViJApps.CanvasTexture.TestSpace
{
    public class PolyLineTestSettings
    {
        public float StrokeThickness;
        public Color StrokeColor;
        public Color FillColor;
        public float StrokeOffset;
        public LineJoinType LineJoinType;
        public float2 Center;
        public float Radius;
        public float RotationSpeed;
        public float TimeOffset;

        public List<float2> PrepareStar()
        {
            //Draw a star
            var points = new List<float2> { new float2(Center.x + Radius, Center.y) };
            for (int i = 1; i < 8; ++i)
            {
                var a = 2.6927937f * i;
                points.Add(new float2(Center.x + Radius * math.cos(a), Center.y + Radius * math.sin(a)));
            }

            return points;
        }
    }

    [Serializable]
    public class TestTextureSettings
    {
        public int Width = 1024;
        public int Height = 1024;
        public float Aspect = 1;
        public Color BackgroundColor = Color.white;
    }

    public class TextureDataExample : MonoBehaviour
    {
        private CanvasTexture m_CanvasTexture;

        private List<PolyLineTestSettings> m_testPolyLines = new List<PolyLineTestSettings>();
        [SerializeField] private TestTextureSettings m_TextureSettings;

        [SerializeField] private Renderer m_testRenderer;

        private float m_Rotation = 0f;

        private void Start()
        {
            for (int i = 0; i < 10; i++)
            {
                var polyLine = new PolyLineTestSettings()
                {
                    TimeOffset = UnityEngine.Random.Range(0f,1f),
                    RotationSpeed = UnityEngine.Random.Range(0.5f, 1f),
                    Center = new float2(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f)),
                    Radius = UnityEngine.Random.Range(0.1f, 0.4f),
                    StrokeThickness = UnityEngine.Random.Range(0.01f, 0.05f),
                    StrokeColor = UnityEngine.Random.ColorHSV(),
                    FillColor = UnityEngine.Random.ColorHSV(),
                };
                m_testPolyLines.Add((polyLine));
            }
        }

        private void Update()
        {
            //Reinit texture (if needed) and clear it
            m_CanvasTexture ??= new CanvasTexture();
            m_CanvasTexture.Init(m_TextureSettings.Width, m_TextureSettings.Height);
            m_CanvasTexture.AspectSettings.Aspect = m_TextureSettings.Aspect;
            m_CanvasTexture.ClearWithColor(m_TextureSettings.BackgroundColor);

            //Prepare star points
            m_Rotation += Time.deltaTime;
            foreach (var polyLine in m_testPolyLines)
            {
                var offsetMatrix = MathUtils.CreateMatrix2d_T(-polyLine.Center);
                var offsetBackMatrix = math.inverse(offsetMatrix);
                var rotationMatrix = MathUtils.CreateMatrix2d_R(m_Rotation);
                var rotateAroundMatrix = math.mul(offsetBackMatrix, math.mul(rotationMatrix, offsetMatrix));

                var star = polyLine.PrepareStar();
                for (int i = 0; i < star.Count; i++)
                {
                    star[i] = star[i].TransformPoint(rotateAroundMatrix);
                }

                m_CanvasTexture.DrawComplexPolygon(
                    new List<List<float2>>() { star },
                    polyLine.StrokeThickness,
                    polyLine.FillColor,
                    polyLine.StrokeColor,
                    polyLine.StrokeOffset,
                    polyLine.LineJoinType
                );
            }

            //Apply operations
            m_CanvasTexture.Flush();

            //Export to texture
            m_testRenderer.material.mainTexture = m_CanvasTexture.RenderTexture;
        }
    }
}