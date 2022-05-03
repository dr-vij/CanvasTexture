using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace ViJ.GraphEditor
{
    public class BackgroundElement : VisualElement
    {
        private GraphElement m_Graph;

        /// <summary>
        /// Grid size in blackboard space
        /// </summary>
        public float GridSize { get; set; } = 25;

        public float GridLineWidth { get; set; } = 2f;

        public Color GridLineColor { get; set; } = Color.gray;

        public BackgroundElement(GraphElement graph)
        {
            AddToClassList("graphBackground");
            m_Graph = graph;
            this.StretchToParentSize();
            generateVisualContent += OnGenerateVisualContent;
            graph.GraphTransformChangeEvent += (_) => MarkDirtyRepaint();
        }

        private void OnGenerateVisualContent(MeshGenerationContext mgc)
        {
            if (GridSize <= 1)
                return;

            //Calculate grid parameters
            var blackBoardStart = m_Graph.RootPointToBlackboard(Vector2.zero) / GridSize;
            blackBoardStart = new Vector2(Mathf.Floor(blackBoardStart.x), Mathf.Floor(blackBoardStart.y)) * GridSize;
            var rootStart = m_Graph.BlackboardPointToRoot(blackBoardStart);
            var rootDelta = m_Graph.BlackboardDeltaToRoot(Vector2.one * GridSize);

            //Prepare painter
            var painter = mgc.painter2D;
            painter.lineWidth = GridLineWidth;
            painter.strokeColor = GridLineColor;
            painter.BeginPath();

            //Vertical lines
            var currentStart = rootStart;
            var currentEnd = new Vector2(rootStart.x, contentRect.height);
       
            while (currentStart.x <= contentRect.width)
            {
                painter.MoveTo(currentStart);
                painter.LineTo(currentEnd);
                currentStart += new Vector2(rootDelta.x, 0);
                currentEnd += new Vector2(rootDelta.x, 0);
            }

            //Horizontal lines
            currentStart = rootStart;
            currentEnd = new Vector2(contentRect.width, rootStart.y);
            while (currentStart.y <= contentRect.height)
            {
                painter.MoveTo(currentStart);
                painter.LineTo(currentEnd);
                currentStart += new Vector2(0, rootDelta.y);
                currentEnd += new Vector2(0, rootDelta.y);
            }

            //Apply painter
            painter.Stroke();
        }
    }
}