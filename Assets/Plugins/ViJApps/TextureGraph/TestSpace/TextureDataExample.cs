using System;
using System.Linq;
using System.Text.RegularExpressions;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

namespace ViJApps.TextureGraph.TestSpace
{
    public class TextureDataExample : MonoBehaviour
    {
        private TextureData m_textureData;
        
        [SerializeField] private Texture2D m_testTexture;
        [SerializeField] private Renderer m_testRenderer;

        [SerializeField] private float m_lineThickness = 64;
        [SerializeField] private float m_aspect = 1f;

        [SerializeField] private float2 m_point0 = new float2(0f, 0.5f);
        [SerializeField] private float2 m_point1 = new float2(1f, 0.5f);
        [SerializeField] private float m_thickness = 0.1f;

        [SerializeField] private float2 m_circlePosition;
        [SerializeField] private float m_circleRadius = 0.1f;

        private void Update()
        {
            var res = CodeGenTest.CodeGenResultTest.Result;
            
            m_textureData = m_textureData ?? new TextureData();

            var w = 1024;
            var h = 512;
            var offset = 64f;

            m_textureData.Init(w, h);
            m_textureData.Aspect = m_aspect;
            m_textureData.ClearWithColor(Color.blue);

            m_textureData.DrawLinePixels(new float2(0 + offset, h - offset), new float2(w - offset, 0 + offset),
                m_lineThickness, Color.yellow, SimpleLineEndingStyle.Round);
            m_textureData.DrawLinePixels(new float2(0 + offset, 0 + offset), new float2(w - offset, h - offset),
                m_lineThickness, Color.grey);

            m_textureData.DrawLinePercent(m_point0, m_point1, m_thickness, Color.black, SimpleLineEndingStyle.Round);
            
            //m_textureData.DrawCirclePercent(m_circlePosition, m_circleRadius, Color.cyan);
            
            m_textureData.DrawEllipsePercent(m_circlePosition, new float2(m_circleRadius, m_circleRadius), Color.cyan);
            m_textureData.DrawText("Fuck");
            //m_TextureData.DrawLinePixels(mPoint0, mPoint1, m_LineThickness, Color.grey);
            m_textureData.Flush();

            m_testTexture = m_textureData.ToTexture2D(m_testTexture);
            m_testRenderer.material.mainTexture = m_testTexture;
        }
    }
}