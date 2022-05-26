using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.Mathematics;
using UnityEngine;
using ViJ.GraphEditor.MathUtils;

namespace ViJ.GraphEditor.TestPurpose
{
    public class TestCubicBezier : MonoBehaviour
    {
        [SerializeField] private LineRenderer m_LineRendererCubic = default;
        [SerializeField] private Transform mTestObj = default;

        private (float2 min, float2 max) m_MinMax;
        private CubicBezierSegment2D test;

        void Start()
        {
            var p0 = new float2(1.1f, 1.5f);
            var p1 = new float2(0.25f, 1.9f);
            var p2 = new float2(2.1f, 2.5f);
            var p3 = new float2(2.1f, 0.3f);

            test = new CubicBezierSegment2D(p0, p1, p2, p3);

            var points = test.FlattenSpline(24);
            m_LineRendererCubic.positionCount = points.Count;
            for (int i = 0; i < points.Count; i++)
            {
                var point = (Vector2)points[i].Position;
                m_LineRendererCubic.SetPosition(i, point);
            }
        }

        private void Test2()
        {
            var p0 = new float2(1.1f, 1.5f);
            var p1 = new float2(0.25f, 1.9f);
            var p2 = new float2(2.1f, 2.5f);
            var p3 = new float2(2.1f, 0.3f);

            var cubic = new CubicBezierSegment2D(p0, p1, p2, p3);
            m_MinMax = cubic.GetMinMax();

            var points = cubic.FlattenSpline(24);
            m_LineRendererCubic.positionCount = points.Count;
            for (int i = 0; i < points.Count; i++)
            {
                var point = (Vector2)points[i].Position;
                m_LineRendererCubic.SetPosition(i, point);
            }
        }

        private void OnDrawGizmos()
        {
            if (Application.isPlaying)
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                float2 result = float2.zero;
                test.DistanceTo((Vector2)mTestObj.position, out result);
                sw.Stop();
                UnityEngine.Debug.Log(sw.ElapsedMilliseconds);

                var size = m_MinMax.max - m_MinMax.min;
                var center = (m_MinMax.min + m_MinMax.max) * 0.5f;
            }
        }
    }
}