using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace ViJ.GraphEditor
{
    public delegate void NodeDragAction(NodeElement node, Vector2 pointerPosition);

    public class NodeInputModule : GenericInputModule<NodeElement>
    {
        private bool m_DragStarted;

        public event NodeDragAction NodeDragStartEvent;
        public event NodeDragAction NodeDragEvent;
        public event NodeDragAction NodeDragEndEvent;

        public NodeInputModule(NodeElement node) : base(node)
        {
        }

        protected override void OnSubscribeEvents(NodeElement node)
        {
            node.RegisterCallback<MouseDownEvent>(OnMouseDown);
            node.RegisterCallback<MouseMoveEvent>(OnMouseMove);
            node.RegisterCallback<MouseUpEvent>(OnMouseUp);
        }

        protected override void OnUnsubscribeEvents(NodeElement node)
        {
            node.UnregisterCallback<MouseDownEvent>(OnMouseDown);
            node.UnregisterCallback<MouseMoveEvent>(OnMouseMove);
            node.UnregisterCallback<MouseUpEvent>(OnMouseUp);
        }

        private void OnMouseDown(MouseDownEvent evt)
        {
            if (CanHandleEvent(evt, true))
            {
                m_TypedTarget.CaptureMouse();
                m_DragStarted = true;
                NodeDragStartEvent?.Invoke(m_TypedTarget, evt.mousePosition);
            }
        }

        private void OnMouseMove(MouseMoveEvent evt)
        {
            if (CanHandleEvent(evt) && m_DragStarted)
            {
                NodeDragEvent?.Invoke(m_TypedTarget, evt.mousePosition);
            }
        }

        private void OnMouseUp(MouseUpEvent evt)
        {
            if (CanHandleEvent(evt) && m_DragStarted)
            {
                m_DragStarted = false;
                m_TypedTarget.ReleaseMouse();
                NodeDragEndEvent?.Invoke(m_TypedTarget, evt.mousePosition);
            }
        }
    }
}