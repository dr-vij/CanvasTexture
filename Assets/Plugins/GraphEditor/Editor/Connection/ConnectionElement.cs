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
        private Vector2 mNoPinWorldPosition0;
        private Vector2 mNoPinWorldPosition1;

        private CubicBezierSegment2D m_Spline;
        private List<BezierSamplePoint> mFlattennedSpline = new List<BezierSamplePoint>();

        private float m_LineWidth = 1f;
        private Color m_LineColor = Color.white;
        private bool m_IsHover;

        public Vector2 NoPinWorldPosition0
        {
            get => mNoPinWorldPosition0;
            set
            {
                mNoPinWorldPosition0 = value;
                MarkForRecalc();
            }
        }

        public Vector2 NoPinWorldPosition1
        {
            get => mNoPinWorldPosition1;
            set
            {
                mNoPinWorldPosition1 = value;
                MarkForRecalc();
            }
        }

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
        }

        public void SetPins(NodePinElement pin0, NodePinElement pin1)
        {
            m_Pin0 = pin0;
            m_Pin1 = pin1;
            m_Pin0.PinPositionChangeEvent += MarkForRecalc;
            m_Pin1.PinPositionChangeEvent += MarkForRecalc;
            RegisterHovering();
            MarkForRecalc();
        }

        private void MarkForRecalc()
        {
            schedule.Execute(RecalculateSpline);
        }

        private void RecalculateSpline()
        {
            //we get pin position from pin or from user set coords
            var fromPosition = this.WorldToLocal(m_Pin0 != null ? m_Pin0.PinWorldPosition : mNoPinWorldPosition0);
            var toPosition = this.WorldToLocal(m_Pin1 != null ? m_Pin1.PinWorldPosition : mNoPinWorldPosition1);

            var min = m_GraphElement.TransformPositionWorldToBlackboard(Vector2.Min(fromPosition, toPosition));
            transform.position = min;
            var delta = fromPosition - toPosition;
            delta = new Vector2(Mathf.Abs(delta.x), Mathf.Abs(delta.y));
            style.height = delta.y;
            style.width = delta.x;

            MarkDirtyRepaint();
        }

        //Overrided this to check if pointer is close enougth to hover
        public override bool ContainsPoint(Vector2 localPoint)
        {
            return m_Spline.DistanceTo(localPoint, out _) < m_LineWidth / 2;
        }

        private void OnMeshGenerationContext(MeshGenerationContext context)
        {
            //we get pin position from pin or from user set coords
            var fromPosition = this.WorldToLocal(m_Pin0 != null ? m_Pin0.PinWorldPosition : mNoPinWorldPosition0);
            var toPosition = this.WorldToLocal(m_Pin1 != null ? m_Pin1.PinWorldPosition : mNoPinWorldPosition1);

            //prepare the painter to draw bezier curve
            var painter = context.painter2D;
            painter.lineWidth = m_IsHover ? m_LineWidth * 1.5f : m_LineWidth;
            painter.strokeColor = m_IsHover ? Color.white : Color.gray * 1.9f;
            painter.lineCap = LineCap.Round;
            painter.BeginPath();

            //Bezier parameters TODO: make spline offset dynamic
            var defaultOffset = 50f;
            var distanceFromStartToEnd = Vector2.Distance(fromPosition, toPosition);
            var offsetDistance = Mathf.Min(defaultOffset, distanceFromStartToEnd);
            var offset = Vector2.right * offsetDistance;

            m_Spline = new CubicBezierSegment2D(fromPosition, fromPosition + offset, toPosition - offset, toPosition);
            m_Spline.FlattenSpline(40, mFlattennedSpline);

            //draw the spline
            painter.MoveTo(mFlattennedSpline[0].Position);
            for (int i = 1; i < mFlattennedSpline.Count; i++)
            {
                painter.LineTo(mFlattennedSpline[i].Position);
                painter.MoveTo(mFlattennedSpline[i].Position);
            }
            painter.Stroke();
        }

        //TODO: unregister
        private void RegisterHovering()
        {
            RegisterCallback<MouseEnterEvent>(OnMouseEnter);
            RegisterCallback<MouseLeaveEvent>(OnMouseLeave);
        }

        //TODO: need something smarter here.
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