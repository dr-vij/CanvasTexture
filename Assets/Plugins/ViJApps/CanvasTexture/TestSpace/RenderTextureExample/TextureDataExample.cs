using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

namespace ViJApps.CanvasTexture.TestSpace
{
    public class TextureDataExample : MonoBehaviour
    {
        private CanvasTexture m_CanvasTexture;

        [SerializeField] private Texture2D m_testTexture;
        [SerializeField] private Renderer m_testRenderer;

        [SerializeField] private float m_lineThickness = 64;
        [SerializeField] private float m_aspect = 1f;

        [SerializeField] private float2 m_point0 = new float2(0f, 0.5f);
        [SerializeField] private float2 m_point1 = new float2(1f, 0.5f);
        [SerializeField] private float m_thickness = 0.1f;

        [Header("EllipseExample")] [SerializeField]
        private float2 m_ellipseCenter = new float2(0.5f, 0.5f);

        [SerializeField] private float2 m_ellipseRadiusAb = new float2(0.2f, 0.2f);
        [SerializeField] private float m_circleStrokeThickness = 0.1f;
        [SerializeField] private Color m_ellipseFillColor = Color.white;
        [SerializeField] private Color m_ellipseStrokeColor = Color.black;
        [Range(0f, 1f)] [SerializeField] private float m_ellipseOffset = 0.5f;

        [Header("Text example")] [SerializeField]
        private TextSettings m_textSettings;

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
            m_CanvasTexture = m_CanvasTexture ?? new CanvasTexture();

            m_points.Clear();
            m_PointsRoot.GetComponentsInChildren<Transform>().Where(c => c != m_PointsRoot).ToList().ForEach(c => { m_points.Add(new float2(c.position.x, c.position.y)); });

            var w = 1024;
            var h = 512;
            var offset = 64f;

            m_CanvasTexture.Init(w, h);
            m_CanvasTexture.AspectSettings.Aspect = m_aspect;
            m_CanvasTexture.ClearWithColor(Color.blue);

            m_CanvasTexture.DrawLinePixels(new float2(0 + offset, h - offset), new float2(w - offset, 0 + offset), m_lineThickness, Color.yellow, SimpleLineEndingStyle.Round);
            m_CanvasTexture.DrawLinePixels(new float2(0 + offset, 0 + offset), new float2(w - offset, h - offset), m_lineThickness, Color.grey);

            m_CanvasTexture.DrawLinePercent(m_point0, m_point1, m_thickness, Color.black, SimpleLineEndingStyle.Round);

            //m_textureData.DrawCirclePercent(m_circlePosition, m_circleRadius, Color.cyan);

            //Example of ellipse
            m_CanvasTexture.DrawEllipsePercent(m_ellipseCenter, m_ellipseRadiusAb, m_circleStrokeThickness, m_ellipseFillColor, m_ellipseStrokeColor, m_ellipseOffset);

            //ExampleOfText
            m_CanvasTexture.DrawText("Test Text", m_textSettings, m_position, m_size, m_rotation);

            // var list = new List<List<float2>> { m_points };
            //m_textureData.DrawSimplePolygon(list, Color.white);
            //m_CanvasTexture.DrawComplexPolygon(list, m_polyLineWidth, m_fillColor, m_polyLineColor, joinType: LineJoinType.Round);

            //m_TextureData.DrawLinePixels(mPoint0, mPoint1, m_LineThickness, Color.grey);
            m_CanvasTexture.Flush();

            m_testTexture = m_CanvasTexture.ToTexture2D(m_testTexture);
            m_testRenderer.material.mainTexture = m_testTexture;
        }
    }
}