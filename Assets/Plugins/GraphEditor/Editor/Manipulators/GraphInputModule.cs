using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace ViJ.GraphEditor
{
    public class GraphInputModule : GenericInputModule<GraphElement>
    {
        private bool m_IsDragStarted;
        private bool m_IsSelectionStarted;
        private Vector2 m_MouseDragStartPosition;

        public float ScaleSensetivity { get; set; } = 0.01f;

        public float MoveSensetivity { get; set; } = 3f;

        public Vector2 MinMaxScale { get; set; } = new Vector2(0.01f, 100);

        public GraphInputModule(GraphElement graph) : base(graph)
        {
        }

        #region Callbacks Register/Unregister

        protected override void OnSubscribeEvents(GraphElement handler)
        {
            //Scale/move with touchpad (optional)
            handler.RegisterCallback<WheelEvent>(OnWheel);

            //Grab
            handler.RegisterCallback<MouseDownEvent>(OnMouseDown);
            handler.RegisterCallback<MouseMoveEvent>(OnMouseMove);
            handler.RegisterCallback<MouseUpEvent>(OnMouseUp);
        }

        protected override void OnUnsubscribeEvents(GraphElement handler)
        {
            handler.UnregisterCallback<WheelEvent>(OnWheel);

            handler.UnregisterCallback<MouseDownEvent>(OnMouseDown);
            handler.UnregisterCallback<MouseMoveEvent>(OnMouseMove);
            handler.UnregisterCallback<MouseUpEvent>(OnMouseUp);
        }

        #endregion

        #region Input handling

        private void OnMouseDown(MouseDownEvent evt)
        {
            if (!CanHandleEvent(evt, true))
                return;

            Debug.Log("GraphDown");
            if (evt.commandKey)
            {
                m_IsDragStarted = true;
                m_MouseDragStartPosition = m_TypedTarget.WorldPointToBlackboard(evt.mousePosition);
                m_TypedTarget.CaptureMouse();
            }
            else
            {
                m_IsSelectionStarted = true;
                m_MouseDragStartPosition = m_TypedTarget.WorldPointToBlackboard(evt.mousePosition);
                var fromTo = m_TypedTarget.WorldPointToRoot(evt.mousePosition);
                m_TypedTarget.StartSelectionBox(fromTo, fromTo);
                m_TypedTarget.CaptureMouse();
            }
        }

        private void OnMouseMove(MouseMoveEvent evt)
        {
            if (!CanHandleEvent(evt))
                return;

            if (m_IsDragStarted)
            {
                var mouseNewPosition = m_TypedTarget.WorldPointToBlackboard(evt.mousePosition);
                var localDelta = mouseNewPosition - m_MouseDragStartPosition;
                var delta = m_TypedTarget.BlackboardDeltaToWorld(localDelta);
                m_TypedTarget.Position += delta;
            }
            else if (m_IsSelectionStarted)
            {
                var from = m_TypedTarget.BlackboardPointToRoot(m_MouseDragStartPosition);
                var to = m_TypedTarget.WorldPointToRoot(evt.mousePosition);
                m_TypedTarget.UpdateSelectionBox(from, to);
            }
        }

        private void OnMouseUp(MouseUpEvent evt)
        {
            if (!CanHandleEvent(evt))
                return;

            Debug.Log("GraphUp");

            if (m_IsDragStarted)
            {
                m_TypedTarget.ReleaseMouse();
                m_IsDragStarted = false;
            }
            else if (m_IsSelectionStarted)
            {
                m_TypedTarget.ReleaseMouse();
                var from = m_TypedTarget.BlackboardPointToRoot(m_MouseDragStartPosition);
                var to = m_TypedTarget.WorldPointToRoot(evt.mousePosition);
                m_TypedTarget.EndSelectionBox(from, to);
                m_IsSelectionStarted = false;
            }
        }

        private void OnWheel(WheelEvent evt)
        {
            if (!CanHandleEvent(evt))
                return;

            //Move or scale. Option/alt swaps x/y
            if (evt.commandKey)
                Scale(evt.mousePosition, evt.delta.y);
            else if (evt.altKey)
                m_TypedTarget.Position -= new Vector2(evt.delta.y, evt.delta.x) * MoveSensetivity;
            else
                m_TypedTarget.Position -= new Vector2(evt.delta.x, evt.delta.y) * MoveSensetivity;

            //Update selection box if needed
            if (m_IsSelectionStarted)
            {
                var from = m_TypedTarget.BlackboardPointToRoot(m_MouseDragStartPosition);
                var to = m_TypedTarget.WorldPointToRoot(evt.mousePosition);
                m_TypedTarget.UpdateSelectionBox(from, to);
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
            var mousePosBefore = m_TypedTarget.WorldPointToRoot(pointerWorldPos);
            var mouseLocalPosBefore = m_TypedTarget.WorldPointToBlackboard(pointerWorldPos);

            //Scale
            var currentScale = m_TypedTarget.Scale;
            var delta = Mathf.Clamp(1 + ScaleSensetivity * scaleDelta, 0.5f, 1.5f);
            var wantedScale = currentScale * delta;
            var clampedScale = Mathf.Clamp(wantedScale, MinMaxScale.x, MinMaxScale.y);
            m_TypedTarget.Scale = clampedScale;

            //Now reposition
            var mousePosAfter = m_TypedTarget.BlackboardPointToRoot(mouseLocalPosBefore);
            var moveDelta = mousePosAfter - mousePosBefore;
            m_TypedTarget.Position -= moveDelta;
        }
    }
}