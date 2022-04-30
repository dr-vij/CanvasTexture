using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace ViJ.GraphEditor
{
    public enum WheelMode
    {
        Unknown = 0,
        WheelIsScale = 1,
        WheelIsMovement = 2,
    }

    public class GraphManipulator : Manipulator
    {
        private GraphElement m_Graph;
        private VisualElement m_Root;

        public WheelMode WheelMode { get; set; } = WheelMode.WheelIsMovement;

        public float ScaleSensetivity { get; set; } = 0.01f;

        public float MoveSensetivity { get; set; } = 1f;

        public Vector2 MinMaxScale { get; set; } = new Vector2(0.01f, 100);

        public GraphManipulator(GraphElement graph)
        {
            m_Graph = graph;
            m_Root = graph.parent;
            target = graph;
        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<WheelEvent>(OnWheel);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<WheelEvent>(OnWheel);
        }

        private void OnWheel(WheelEvent evt)
        {
            switch (WheelMode)
            {
                case WheelMode.WheelIsMovement:
                    if (evt.altKey)
                        Scale(evt.mousePosition, evt.delta.y);
                    else
                        m_Graph.Position -= (Vector2)(evt.delta * MoveSensetivity);
                    break;
                case WheelMode.WheelIsScale:
                    Scale(evt.mousePosition, evt.delta.y);
                    break;
                default:
                    throw new System.ArgumentException("Unknown wheel mode");
            }
        }

        private void Scale(Vector2 pointerPosition, float scaleDelta)
        {
            //Save position of pointer before scale
            var mousePosBefore = pointerPosition;
            var mouseLocalPosBefore = m_Graph.WorldToBlackboard(mousePosBefore);

            //Scale
            var currentScale = m_Graph.Scale;
            var delta = Mathf.Clamp(1 + ScaleSensetivity * scaleDelta, 0.5f, 1.5f);
            var wantedScale = currentScale * delta;
            var clampedScale = Mathf.Clamp(wantedScale, MinMaxScale.x, MinMaxScale.y);
            m_Graph.Scale = clampedScale;

            //Now reposition
            var mousePosAfter = m_Graph.BlackboardToWorld(mouseLocalPosBefore);
            var moveDelta = mousePosAfter - mousePosBefore;
            m_Graph.Position -= moveDelta;
        }
    }
}