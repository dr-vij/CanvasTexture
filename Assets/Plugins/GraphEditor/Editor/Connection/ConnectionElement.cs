using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

using ViJ.GraphEditor.MathUtils;

namespace ViJ.GraphEditor
{
    public class ConnectionElement : VisualElement
    {
        private NodePinElement m_Pin0;
        private NodePinElement m_Pin1;

        private CubicBezierSegment2D m_Spline;
        private List<BezierSamplePoint> mFlattennedSpline = new List<BezierSamplePoint>();

        private Vector2 mTestPosition;

        public ConnectionElement()
        {
            generateVisualContent += OnMeshGenerationContext;
            style.position = Position.Absolute;
        }

        public void SetPins(NodePinElement pin0, NodePinElement pin1)
        {
            m_Pin0 = pin0;
            m_Pin1 = pin1;

            m_Pin0.PinPositionChangeEvent += () => OnNeedRefreshPosition();
            m_Pin1.PinPositionChangeEvent += () => OnNeedRefreshPosition();

            MarkDirtyRepaint();
        }

        private void OnNeedRefreshPosition()
        {
            var min = Vector2.Min(m_Pin0.PinWorldPosition, m_Pin1.PinWorldPosition);
            transform.position = min;
            var delta = m_Pin0.PinWorldPosition - m_Pin1.PinWorldPosition;
            delta = new Vector2(Mathf.Abs(delta.x), Mathf.Abs(delta.y));
            style.height = delta.y;
            style.width = delta.x;

            MarkDirtyRepaint();
        }

        public void SetTestPoint(Vector2 worldPos)
        {
            mTestPosition = worldPos;
            MarkDirtyRepaint();
        }

        public override bool ContainsPoint(Vector2 localPoint)
        {
            return true;
        }

        public override bool Overlaps(Rect rectangle)
        {
            return true;
        }

        private void OnMeshGenerationContext(MeshGenerationContext context)
        {
            if (m_Pin0 == null || m_Pin1 == null)
                return;

            var painter = context.painter2D;
            painter.lineWidth = 2f;
            painter.strokeColor = Color.white;
            painter.lineCap = LineCap.Round;
            painter.BeginPath();

            var fromPosition = this.WorldToLocal(m_Pin0.PinWorldPosition);
            var toPosition = this.WorldToLocal(m_Pin1.PinWorldPosition);

            var scale = 50f;
            var offset = Vector2.right * scale;
            m_Spline = new CubicBezierSegment2D(fromPosition, fromPosition + offset, toPosition - offset, toPosition);
            m_Spline.FlattenSpline(40, mFlattennedSpline);

            painter.MoveTo(mFlattennedSpline[0].Position);
            for (int i = 1; i < mFlattennedSpline.Count; i++)
            {
                painter.LineTo(mFlattennedSpline[i].Position);
                painter.MoveTo(mFlattennedSpline[i].Position);
            }

            var pos = this.WorldToLocal(mTestPosition);
            var radius = 20f;
            painter.MoveTo(pos + Vector2.right * radius);
            painter.Arc(pos, radius, new Angle(0), new Angle(360));

            painter.Stroke();
        }
    }
}