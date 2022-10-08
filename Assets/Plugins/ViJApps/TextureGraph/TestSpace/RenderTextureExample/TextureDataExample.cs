using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;
using ViJApps.TextureGraph.Utils;

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

        [SerializeField] private TextSettings m_textSettings;

        [SerializeField] private float2 m_position;
        [SerializeField] private float2 m_size;
        [SerializeField] private float m_rotation;

        [SerializeField] private float m_polyLineWidth = 0.05f;
        [SerializeField] private Color m_polyLineColor = Color.white;
        [SerializeField] private Color m_fillColor = Color.red;
        [SerializeField] private Transform m_PointsRoot; 
        
        private List<float2> m_points = new List<float2>();
        
        private void Update()
        {
            m_textureData = m_textureData ?? new TextureData();

            m_points.Clear();
            m_PointsRoot.GetComponentsInChildren<Transform>().Where(c=>c != m_PointsRoot).ToList().ForEach(c =>
            {
                m_points.Add(new float2(c.position.x, c.position.y));
            });
            
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
            m_textureData.DrawText("Test Text", m_textSettings, m_position, m_size, m_rotation);

            var list = new List<List<float2>> { m_points };
            //m_textureData.DrawSimplePolygon(list, Color.white);
            m_textureData.DrawComplexPolygon(list, m_polyLineWidth, m_fillColor, m_polyLineColor, joinType: LineJoinType.Round);
            
            //m_TextureData.DrawLinePixels(mPoint0, mPoint1, m_LineThickness, Color.grey);
            m_textureData.Flush();

            m_testTexture = m_textureData.ToTexture2D(m_testTexture);
            m_testRenderer.material.mainTexture = m_testTexture;
        }
    }
}