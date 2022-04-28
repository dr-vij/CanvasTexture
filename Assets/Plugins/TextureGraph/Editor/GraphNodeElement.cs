using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.IO;

namespace ViJ.GraphEditor
{
    public class GraphNodeElement : VisualElement
    {
        private const string UXML = nameof(GraphNodeElement) + ".uxml";

        public new class UxmlFactory : UxmlFactory<GraphNodeElement, UxmlTraits> { }

        public new class UxmlTraits : VisualElement.UxmlTraits { }

        public GraphNodeElement()
        {
            var asset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(Path.Combine(TextureGraphSettings.PLUGIN_PATH, "Editor", UXML));
            Add(asset.Instantiate());
        }
    }
}