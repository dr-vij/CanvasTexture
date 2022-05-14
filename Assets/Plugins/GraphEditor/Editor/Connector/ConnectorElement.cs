using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace ViJ.GraphEditor
{
    public class ConnectionElement : VisualElement
    {
        private NodePinElement m_Pin0;
        private NodePinElement m_Pin1;

        public ConnectionElement()
        {
            generateVisualContent += OnMeshGenerationContext;
        }

        public void SetPins(NodePinElement pin0, NodePinElement pin1)
        {
            m_Pin0 = pin0;
            m_Pin1 = pin1;

            m_Pin0.PinPositionChangeEvent += () => MarkDirtyRepaint();
            m_Pin1.PinPositionChangeEvent += () => MarkDirtyRepaint();

            MarkDirtyRepaint();
        }

        private void OnMeshGenerationContext(MeshGenerationContext context)
        {
            if (m_Pin0 == null || m_Pin1 == null)
                return;

            var painter = context.painter2D;
            painter.BeginPath();

            var fromPosition = this.WorldToLocal(m_Pin0.PinWorldPosition);
            var toPosition = this.WorldToLocal(m_Pin1.PinWorldPosition);

            painter.MoveTo(fromPosition);
            painter.LineTo(toPosition);
            painter.Stroke();
        }
    }
}