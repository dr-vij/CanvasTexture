using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace ViJ.GraphEditor
{
    public class NodeInputModule : GenericInputModule<NodeElement>
    {
        private GraphElement m_Graph;

        private bool m_DragStarted;
        private Vector2 m_MouseDragStartPosition;
        private Vector2 m_NodeDragStartPosition;

        public NodeInputModule(NodeElement node, GraphElement graph) : base(node)
        {
            m_Graph = graph;
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
            if (!CanHandleEvent(evt, true))
                return;

            Debug.Log("NodeDown");

            m_TypedTarget.CaptureMouse();
            m_DragStarted = true;
            m_MouseDragStartPosition = evt.mousePosition;
            m_NodeDragStartPosition = m_TypedTarget.BlackboardPosition;
        }

        private void OnMouseMove(MouseMoveEvent evt)
        {
            if (!CanHandleEvent(evt))
                return;

            if (m_DragStarted)
            {
                var mouseDelta = evt.mousePosition - m_MouseDragStartPosition;
                var blackboardDelta = m_Graph.WorldDeltaToBlackboard(mouseDelta);
                m_TypedTarget.BlackboardPosition = m_NodeDragStartPosition + blackboardDelta;
            }
        }

        private void OnMouseUp(MouseUpEvent evt)
        {
            if (!CanHandleEvent(evt))
                return;

            Debug.Log("NodeUp");

            if (m_DragStarted)
            {
                m_DragStarted = false;
                m_TypedTarget.ReleaseMouse();
            }
        }
    }
}