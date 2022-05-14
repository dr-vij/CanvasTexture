using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace ViJ.GraphEditor
{
    public delegate void DragHandler(Vector2 pointerPosition);

    public class DragInputModule : InputModuleBase
    {
        private bool m_DragStarted;

        public event DragHandler NodeDragStartEvent;
        public event DragHandler NodeDragEvent;
        public event DragHandler NodeDragEndEvent;

        public DragInputModule(VisualElement node) : base(node)
        {
        }

        protected override void SubscribeEvents(VisualElement handler)
        {
            handler.RegisterCallback<MouseDownEvent>(OnMouseDown);
            handler.RegisterCallback<MouseMoveEvent>(OnMouseMove);
            handler.RegisterCallback<MouseUpEvent>(OnMouseUp);
        }

        protected override void UnsubscribeEvents(VisualElement node)
        {
            node.UnregisterCallback<MouseDownEvent>(OnMouseDown);
            node.UnregisterCallback<MouseMoveEvent>(OnMouseMove);
            node.UnregisterCallback<MouseUpEvent>(OnMouseUp);
        }

        private void OnMouseDown(MouseDownEvent evt)
        {
            if (CanHandleEvent(evt, true))
            {
                m_Target.CaptureMouse();
                m_DragStarted = true;
                NodeDragStartEvent?.Invoke(evt.mousePosition);
            }
        }

        private void OnMouseMove(MouseMoveEvent evt)
        {
            if (CanHandleEvent(evt) && m_DragStarted)
            {
                NodeDragEvent?.Invoke(evt.mousePosition);
            }
        }

        private void OnMouseUp(MouseUpEvent evt)
        {
            if (CanHandleEvent(evt) && m_DragStarted)
            {
                m_DragStarted = false;
                m_Target.ReleaseMouse();
                NodeDragEndEvent?.Invoke(evt.mousePosition);
            }
        }
    }
}