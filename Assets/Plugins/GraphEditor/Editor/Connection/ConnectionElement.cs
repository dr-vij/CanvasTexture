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

        private GraphElement m_GraphElement;

        private CubicBezierSegment2D m_Spline;
        private List<BezierSamplePoint> mFlattennedSpline = new List<BezierSamplePoint>();

        private float m_LineWidth = 2f;
        private Color m_LineColor = Color.white;
        private bool m_IsHover;

        public float LineWidth
        {
            get => m_LineWidth;
            set => m_LineWidth = value;
        }

        public Color LineColor
        {
            get => m_LineColor;
            set => m_LineColor = value;
        }

        public ConnectionElement(GraphElement graphElement)
        {
            m_GraphElement = graphElement;

            generateVisualContent += OnMeshGenerationContext;
            style.position = Position.Absolute;
            RegisterHovering();
        }

        public void SetPins(NodePinElement pin0, NodePinElement pin1)
        {
            m_Pin0 = pin0;
            m_Pin1 = pin1;
            m_Pin0.PinPositionChangeEvent += MarkForRecalc;
            m_Pin1.PinPositionChangeEvent += MarkForRecalc;
            MarkForRecalc();
        }

        private void MarkForRecalc()
        {
            schedule.Execute(RecalculateSpline);
        }

        private void RecalculateSpline()
        {
            var min = m_GraphElement.TransformPositionWorldToBlackboard(Vector2.Min(m_Pin0.PinWorldPosition, m_Pin1.PinWorldPosition));
            transform.position = min;
            var delta = m_Pin0.PinWorldPosition - m_Pin1.PinWorldPosition;
            delta = new Vector2(Mathf.Abs(delta.x), Mathf.Abs(delta.y));
            style.height = delta.y;
            style.width = delta.x;

            MarkDirtyRepaint();
        }

        public override bool ContainsPoint(Vector2 localPoint)
        {
            return m_Spline.DistanceTo(localPoint, out _) < m_LineWidth / 2;
        }

        private void OnMeshGenerationContext(MeshGenerationContext context)
        {
            if (m_Pin0 == null || m_Pin1 == null)
                return;

            var painter = context.painter2D;
            painter.lineWidth = m_IsHover ? m_LineWidth * 1.5f : m_LineWidth;
            painter.strokeColor = m_IsHover ? Color.white : Color.gray * 1.9f;
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
            painter.Stroke();
        }

        private void RegisterHovering()
        {
            RegisterCallback<MouseEnterEvent>(OnMouseEnter);
            RegisterCallback<MouseLeaveEvent>(OnMouseLeave);
        }

        private void OnMouseEnter(MouseEnterEvent evt)
        {
            m_IsHover = true;
            MarkDirtyRepaint();
        }

        private void OnMouseLeave(MouseLeaveEvent evt)
        {
            m_IsHover = false;
            MarkDirtyRepaint();
        }
    }
}