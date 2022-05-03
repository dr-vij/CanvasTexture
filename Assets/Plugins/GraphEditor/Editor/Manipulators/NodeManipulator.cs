using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace ViJ.GraphEditor
{
    public class NodeManipulator : GraphMouseManipulator
    {
        private NodeElement m_Node;
        private GraphElement m_Graph;

        private bool m_DragStarted;
        private Vector2 m_MouseDragStartPosition;
        private Vector2 m_NodeDragStartPosition;

        public NodeManipulator(NodeElement node, GraphElement graph)
        {
            m_Node = node;
            m_Graph = graph;
            target = node;
        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<MouseDownEvent>(OnMouseDown);
            target.RegisterCallback<MouseMoveEvent>(OnMouseMove);
            target.RegisterCallback<MouseUpEvent>(OnMouseUp);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<MouseDownEvent>(OnMouseDown);
            target.UnregisterCallback<MouseMoveEvent>(OnMouseMove);
            target.UnregisterCallback<MouseUpEvent>(OnMouseUp);
        }

        private void OnMouseDown(MouseDownEvent evt)
        {
            if (!TryHandleAndStopPropagation(evt, true))
                return;

            Debug.Log("NodeDown");

            target.CaptureMouse();
            m_DragStarted = true;
            m_MouseDragStartPosition = evt.mousePosition;
            m_NodeDragStartPosition = m_Node.BlackboardPosition;
        }

        private void OnMouseMove(MouseMoveEvent evt)
        {
            if (!TryHandleAndStopPropagation(evt))
                return;

            if (m_DragStarted)
            {
                var mouseDelta = evt.mousePosition - m_MouseDragStartPosition;
                var blackboardDelta = m_Graph.WorldDeltaToBlackboard(mouseDelta);
                m_Node.BlackboardPosition = m_NodeDragStartPosition + blackboardDelta;
            }
        }

        private void OnMouseUp(MouseUpEvent evt)
        {
            if (!TryHandleAndStopPropagation(evt))
                return;

            Debug.Log("NodeUp");

            if (m_DragStarted)
            {
                m_DragStarted = false;
                target.ReleaseMouse();
            }
        }
    }
}