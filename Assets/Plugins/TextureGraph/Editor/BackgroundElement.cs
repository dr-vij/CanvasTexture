using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace ViJ.GraphEditor
{
    public class BGElement : VisualElement
    {
        public BGElement()
        {
            generateVisualContent += OnGenerateVisualContent;
        }

        private void OnGenerateVisualContent(MeshGenerationContext mgc)
        {
            var painter = mgc.painter2D;
            painter.BeginPath();
            painter.MoveTo(Vector2.one * 100);
            painter.LineTo(Vector2.one * 100);
            painter.LineTo(Vector2.one * 200);
            painter.Stroke();
        }
    }
}