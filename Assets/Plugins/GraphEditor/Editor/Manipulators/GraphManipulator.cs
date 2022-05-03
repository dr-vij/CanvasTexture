using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace ViJ.GraphEditor
{
    public abstract class GraphMouseManipulator : MouseManipulator
    {
        /// <summary>
        /// This method checks if evt current target is captured and stops propagation if so
        /// </summary>
        /// <param name="evt"></param>
        /// <returns></returns>
        protected bool TryHandleAndStopPropagation(EventBase evt, bool stopPropagationEvenIfNotCaptured = false)
        {
            var isTargetCaptured = evt.currentTarget.HasMouseCapture() && evt.currentTarget == target;
            var canBeHadled = !evt.currentTarget.HasMouseCapture() || isTargetCaptured;
            if (isTargetCaptured || (stopPropagationEvenIfNotCaptured && canBeHadled))
                evt.StopPropagation();
            return canBeHadled;
        }

        /// <summary>
        /// This method checks if evt current event can be handled
        /// </summary>
        /// <param name="evt"></param>
        /// <returns></returns>
        protected bool TryHandle(EventBase evt)
        {
            var canBeHandled = !evt.currentTarget.HasMouseCapture() || evt.currentTarget == target;
            return canBeHandled;
        }
    }

    public class GraphManipulator : GraphMouseManipulator
    {
        private GraphElement m_Graph;
        private bool m_IsDragStarted;
        private bool m_IsSelectionStarted;
        private Vector2 m_MouseDragStartPosition;

        public float ScaleSensetivity { get; set; } = 0.01f;

        public float MoveSensetivity { get; set; } = 3f;

        public Vector2 MinMaxScale { get; set; } = new Vector2(0.01f, 100);

        public GraphManipulator(GraphElement graph)
        {
            m_Graph = graph;
            target = graph;
            activators.Add(new ManipulatorActivationFilter() { button = MouseButton.LeftMouse });
        }

        #region Callbacks Register/Unregister

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
            if (!TryHandleAndStopPropagation(evt, true))
                return;

            Debug.Log("GraphDown");
            if (evt.commandKey)
            {
                m_IsDragStarted = true;
                m_MouseDragStartPosition = m_Graph.WorldPointToBlackboard(evt.mousePosition);
                target.CaptureMouse();
            }
            else
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
            if (!TryHandleAndStopPropagation(evt))
                return;

            if (m_IsDragStarted)
            {
                var mouseNewPosition = m_Graph.WorldPointToBlackboard(evt.mousePosition);
                var localDelta = mouseNewPosition - m_MouseDragStartPosition;
                var delta = m_Graph.BlackboardDeltaToWorld(localDelta);
                m_Graph.Position += delta;
            }
            else if (m_IsSelectionStarted)
            {
                var from = m_Graph.BlackboardPointToRoot(m_MouseDragStartPosition);
                var to = m_Graph.WorldPointToRoot(evt.mousePosition);
                m_Graph.UpdateSelectionBox(from, to);
            }
        }

        private void OnMouseUp(MouseUpEvent evt)
        {
            if (!TryHandleAndStopPropagation(evt))
                return;

            Debug.Log("GraphUp");

            if (m_IsDragStarted)
            {
                target.ReleaseMouse();
                m_IsDragStarted = false;
            }
            else if (m_IsSelectionStarted)
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
            if (!TryHandle(evt))
                return;

            //Move or scale. Option/alt swaps x/y
            if (evt.commandKey)
                Scale(evt.mousePosition, evt.delta.y);
            else if (evt.altKey)
                m_Graph.Position -= new Vector2(evt.delta.y, evt.delta.x) * MoveSensetivity;
            else
                m_Graph.Position -= new Vector2(evt.delta.x, evt.delta.y) * MoveSensetivity;

            //Update selection box if needed
            if (m_IsSelectionStarted)
            {
                var from = m_Graph.BlackboardPointToRoot(m_MouseDragStartPosition);
                var to = m_Graph.WorldPointToRoot(evt.mousePosition);
                m_Graph.UpdateSelectionBox(from, to);
            }
        }

        #endregion

        /// <summary>
        /// Scales graph relative to the current world mouse position
        /// </summary>
        /// <param name="pointerWorldPos"></param>
        /// <param name="scaleDelta"></param>
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
    }
}