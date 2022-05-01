using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ViJ.GraphEditor
{
    public class GraphElement : VisualElement
    {
        private const string UXML = nameof(GraphElement) + ".uxml";
        private const string ELEMENT_PATH = "Editor";
        private const string BG_NAME = "BackgroundRoot";
        private const string BLACKBOARD_NAME = "BlackboardRoot";

        public new class UxmlFactory : UxmlFactory<GraphElement, UxmlTraits> { }
        public new class UxmlTraits : VisualElement.UxmlTraits { }

        private VisualElement m_Root;
        private VisualElement m_BgRoot;
        private VisualElement m_BlackboardRoot;

        public GraphElement()
        {
            var asset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(Path.Combine(TextureGraphSettings.PLUGIN_PATH, ELEMENT_PATH, UXML));
            m_Root = asset.Instantiate();
            m_Root.StretchToParentSize();
            Add(m_Root);

            m_BgRoot = m_Root.Q(BG_NAME);
            m_BlackboardRoot = m_Root.Q(BLACKBOARD_NAME);
        }

        public void AddToBg(VisualElement element)
        {
            m_BgRoot.Add(element);
        }

        public void AddToBlackboard(VisualElement element)
        {
            m_BlackboardRoot.Add(element);
        }

        public Vector2 Position
        {
            get => m_BlackboardRoot.transform.position;
            set => m_BlackboardRoot.transform.position = value;
        }

        public float Scale
        {
            get => m_BlackboardRoot.transform.scale.x;
            set => m_BlackboardRoot.transform.scale = new Vector2(value, value);
        }

        public Vector2 BlackboardPointToWorld(Vector2 localPosition) => m_BlackboardRoot.LocalToWorld(localPosition);

        public Vector2 WorldPointToBlackboard(Vector2 worldPosition) => m_BlackboardRoot.WorldToLocal(worldPosition);

        public Vector2 BlackboardDeltaToWorld(Vector2 localDelta)
        {
        }
    }
}