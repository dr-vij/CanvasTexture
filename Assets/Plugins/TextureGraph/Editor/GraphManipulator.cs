using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace ViJ.GraphEditor
{
    public class GraphManipulator : Manipulator
    {
        private GraphElement m_Graph;
        private bool m_IsDragStarted;
        private Vector2 m_MouseDragStartPosition;

        public float ScaleSensetivity { get; set; } = 0.01f;

        public float MoveSensetivity { get; set; } = 1f;

        public Vector2 MinMaxScale { get; set; } = new Vector2(0.01f, 100);

        public GraphManipulator(GraphElement graph)
        {
            m_Graph = graph;
            target = graph;
        }

        #region Callbacks

        protected override void RegisterCallbacksOnTarget()
        {
            //Scale/move with touchpad (optional)
            target.RegisterCallback<WheelEvent>(OnWheel);

            //Grab
            target.RegisterCallback<MouseDownEvent>(OnMouseDown);
            target.RegisterCallback<MouseMoveEvent>(OnMouseMove);
            target.RegisterCallback<MouseUpEvent>(OnMouseUp);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<WheelEvent>(OnWheel);

            target.UnregisterCallback<MouseDownEvent>(OnMouseDown);
            target.UnregisterCallback<MouseMoveEvent>(OnMouseMove);
            target.UnregisterCallback<MouseUpEvent>(OnMouseUp);
        }

        #endregion

        #region Drag handling

        private void OnMouseDown(MouseDownEvent evt)
        {
            if (evt.commandKey)
            {
                m_IsDragStarted = true;
                m_MouseDragStartPosition = m_Graph.WorldPointToBlackboard(evt.mousePosition);
                target.CaptureMouse();
            }
        }

        private void OnMouseMove(MouseMoveEvent evt)
        {
            if (m_IsDragStarted && target.HasMouseCapture())
            {
                var mouseNewPosition = m_Graph.WorldPointToBlackboard(evt.mousePosition);
                var localDelta = mouseNewPosition - m_MouseDragStartPosition;
                var delta = m_Graph.BlackboardDeltaToWorld(localDelta);
                m_Graph.Position += delta;
            }
        }

        private void OnMouseUp(MouseUpEvent evt)
        {
            if (m_IsDragStarted && target.HasMouseCapture())
            {
                target.ReleaseMouse();
            }
        }

        #endregion

        private void OnWheel(WheelEvent evt)
        {
            if (evt.commandKey)
                Scale(evt.mousePosition, evt.delta.y);
            else if (evt.altKey)
                m_Graph.Position -= new Vector2(evt.delta.y, evt.delta.x) * MoveSensetivity;
            else
                m_Graph.Position -= new Vector2(evt.delta.x, evt.delta.y) * MoveSensetivity;
        }

        private void Scale(Vector2 pointerPosition, float scaleDelta)
        {
            //Save position of pointer before scale
            var mousePosBefore = pointerPosition;
            var mouseLocalPosBefore = m_Graph.WorldPointToBlackboard(mousePosBefore);

            //Scale
            var currentScale = m_Graph.Scale;
            var delta = Mathf.Clamp(1 + ScaleSensetivity * scaleDelta, 0.5f, 1.5f);
            var wantedScale = currentScale * delta;
            var clampedScale = Mathf.Clamp(wantedScale, MinMaxScale.x, MinMaxScale.y);
            m_Graph.Scale = clampedScale;

            //Now reposition
            var mousePosAfter = m_Graph.BlackboardPointToWorld(mouseLocalPosBefore);
            var moveDelta = mousePosAfter - mousePosBefore;
            m_Graph.Position -= moveDelta;
        }
    }
}