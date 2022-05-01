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
        private bool m_IsSelectionStarted;
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

        #region Input handling

        private void OnMouseDown(MouseDownEvent evt)
        {
            if (evt.commandKey && evt.button == 0)
            {
                m_IsDragStarted = true;
                m_MouseDragStartPosition = m_Graph.WorldPointToBlackboard(evt.mousePosition);
                target.CaptureMouse();
            }
            else if (evt.button == 0)
            {
                m_IsSelectionStarted = true;
                m_MouseDragStartPosition = m_Graph.WorldPointToBlackboard(evt.mousePosition);
                var fromTo = m_Graph.WorldPointToRoot(evt.mousePosition);
                m_Graph.StartSelectionBox(fromTo, fromTo);
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
            else if (m_IsSelectionStarted && target.HasMouseCapture())
            {
                var from = m_Graph.BlackboardPointToRoot(m_MouseDragStartPosition);
                var to = m_Graph.WorldPointToRoot(evt.mousePosition);
                m_Graph.UpdateSelectionBox(from, to);
            }
        }

        private void OnMouseUp(MouseUpEvent evt)
        {
            if (m_IsDragStarted && target.HasMouseCapture())
            {
                target.ReleaseMouse();
                m_IsDragStarted = false;
            }
            else if (m_IsSelectionStarted && target.HasMouseCapture())
            {
                target.ReleaseMouse();
                var from = m_Graph.BlackboardPointToRoot(m_MouseDragStartPosition);
                var to = m_Graph.WorldPointToRoot(evt.mousePosition);
                m_Graph.EndSelectionBox(from, to);
                m_IsSelectionStarted = false;
            }
        }

        private void OnWheel(WheelEvent evt)
        {
            //Move or scale. Option/alt swaps x/y
            if (evt.commandKey)
                Scale(evt.mousePosition, evt.delta.y);
            else if (evt.altKey)
                m_Graph.Position -= new Vector2(evt.delta.y, evt.delta.x) * MoveSensetivity;
            else
                m_Graph.Position -= new Vector2(evt.delta.x, evt.delta.y) * MoveSensetivity;

            //Update selection box if needed
            if (m_IsSelectionStarted && target.HasMouseCapture())
            {
                var from = m_Graph.BlackboardPointToRoot(m_MouseDragStartPosition);
                var to = m_Graph.WorldPointToRoot(evt.mousePosition);
                m_Graph.UpdateSelectionBox(from, to);
            }
        }

        private void Scale(Vector2 pointerWorldPos, float scaleDelta)
        {
            //Save position of pointer before scale
            var mousePosBefore = m_Graph.WorldPointToRoot(pointerWorldPos);
            var mouseLocalPosBefore = m_Graph.WorldPointToBlackboard(pointerWorldPos);

            //Scale
            var currentScale = m_Graph.Scale;
            var delta = Mathf.Clamp(1 + ScaleSensetivity * scaleDelta, 0.5f, 1.5f);
            var wantedScale = currentScale * delta;
            var clampedScale = Mathf.Clamp(wantedScale, MinMaxScale.x, MinMaxScale.y);
            m_Graph.Scale = clampedScale;

            //Now reposition
            var mousePosAfter = m_Graph.BlackboardPointToRoot(mouseLocalPosBefore);
            var moveDelta = mousePosAfter - mousePosBefore;
            m_Graph.Position -= moveDelta;
        }

        #endregion
    }
}