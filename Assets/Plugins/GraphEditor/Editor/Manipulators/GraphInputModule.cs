using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace ViJ.GraphEditor
{
    public delegate void GraphDrag(Vector2 position);
    public delegate void Slide(Vector2 delta);
    public delegate void GraphZoom(Vector2 position, float delta);

    public class GraphInputModule : GenericInputModule<GraphElement>
    {
        private bool m_IsDragStarted;
        private bool m_IsSelectionStarted;

        //Drag by pointer
        public event GraphDrag DragStartEvent;
        public event GraphDrag DragEvent;
        public event GraphDrag DragEndEvent;

        //Selection by pointer
        public event GraphDrag RectSelectionStartEvent;
        public event GraphDrag RectSelectionEvent;
        public event GraphDrag RectSelectionEndEvent;

        //Zoom to pointer
        public event GraphZoom ZoomEvent;

        //Slide with wheel or pad
        public event Slide SlideEvent;

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
            if (CanHandleEvent(evt, true))
            {
                m_TypedTarget.CaptureMouse();

                if (evt.commandKey)
                {
                    m_IsDragStarted = true;
                    DragStartEvent?.Invoke(evt.mousePosition);
                }
                else
                {
                    m_IsSelectionStarted = true;
                    RectSelectionStartEvent?.Invoke(evt.mousePosition);
                }
            }
        }

        private void OnMouseMove(MouseMoveEvent evt)
        {
            if (CanHandleEvent(evt))
            {
                if (m_IsDragStarted)
                    DragEvent?.Invoke(evt.mousePosition);
                else if (m_IsSelectionStarted)
                    RectSelectionEvent?.Invoke(evt.mousePosition);
            }
        }

        private void OnMouseUp(MouseUpEvent evt)
        {
            if (CanHandleEvent(evt))
            {
                m_TypedTarget.ReleaseMouse();
                if (m_IsDragStarted)
                {
                    m_IsDragStarted = false;
                    DragEndEvent?.Invoke(evt.mousePosition);
                }
                else if (m_IsSelectionStarted)
                {
                    m_IsSelectionStarted = false;
                    RectSelectionEndEvent?.Invoke(evt.mousePosition);
                }
            }
        }

        private void OnWheel(WheelEvent evt)
        {
            if (!CanHandleEvent(evt))
                return;

            //Move or scale. Option/alt swaps x/y
            if (evt.commandKey)
                ZoomEvent?.Invoke(evt.mousePosition, evt.delta.y);
            else if (evt.altKey)
                SlideEvent?.Invoke(-new Vector2(evt.delta.y, evt.delta.x));
            else
                SlideEvent?.Invoke(-new Vector2(evt.delta.x, evt.delta.y));

            //Raise selection event to update rect
            if (m_IsSelectionStarted)
                RectSelectionEvent?.Invoke(evt.mousePosition);
        }

        #endregion
    }
}