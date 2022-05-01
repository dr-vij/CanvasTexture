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
        private const string SELECTIONBOX_NAME = "SelectionBox";

        public new class UxmlFactory : UxmlFactory<GraphElement, UxmlTraits> { }
        public new class UxmlTraits : VisualElement.UxmlTraits { }

        private VisualElement m_Root;
        private VisualElement m_BgRoot;
        private VisualElement m_BlackboardRoot;
        private VisualElement m_SelectionBox;

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

        public GraphElement()
        {
            var asset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(Path.Combine(TextureGraphSettings.PLUGIN_PATH, ELEMENT_PATH, UXML));
            m_Root = asset.Instantiate();
            m_Root.StretchToParentSize();
            Add(m_Root);

            m_BgRoot = m_Root.Q(BG_NAME);
            m_BlackboardRoot = m_Root.Q(BLACKBOARD_NAME);
            m_SelectionBox = m_Root.Q(SELECTIONBOX_NAME);
        }

        public void AddToBg(VisualElement element)
        {
            m_BgRoot.Add(element);
        }

        public void AddToBlackboard(VisualElement element)
        {
            m_BlackboardRoot.Add(element);
        }

        public void StartSelectionBox(Vector2 from, Vector2 to)
        {
            m_SelectionBox.style.visibility = Visibility.Visible;
            UpdateVisualBox(from, to);
        }

        public void UpdateSelectionBox(Vector2 from, Vector2 to)
        {
            UpdateVisualBox(from, to);
        }

        public void EndSelectionBox(Vector2 from, Vector2 to)
        {
            UpdateVisualBox(from, to);
            m_SelectionBox.style.visibility = Visibility.Hidden;
        }

        private void UpdateVisualBox(Vector2 from, Vector2 to)
        {
            var fromCorrected = Vector2.Min(from, to);
            var toCorrected = Vector2.Max(from, to);
            m_SelectionBox.transform.position = fromCorrected;
            m_SelectionBox.style.width = Mathf.Abs(toCorrected.x - fromCorrected.x);
            m_SelectionBox.style.height = Mathf.Abs(toCorrected.y - fromCorrected.y);
        }

        #region Coords conversion
        public Vector2 BlackboardPointToWorld(Vector2 localPosition) => m_BlackboardRoot.LocalToWorld(localPosition);

        public Vector2 WorldPointToBlackboard(Vector2 worldPosition) => m_BlackboardRoot.WorldToLocal(worldPosition);

        public Vector2 WorldPointToRoot(Vector2 worldPosition) => m_Root.WorldToLocal(worldPosition);

        public Vector2 BlackboardPointToRoot(Vector2 localPosition)
        {
            var worldPos = m_BlackboardRoot.LocalToWorld(localPosition);
            return m_Root.WorldToLocal(worldPos);
        }

        public Vector2 RootPointToBlackboard(Vector2 localPosition)
        {
            var worldPos = m_Root.LocalToWorld(localPosition);
            return m_BlackboardRoot.WorldToLocal(worldPos);
        }

        public Vector2 BlackboardDeltaToWorld(Vector2 localDelta)
        {
            var p1 = m_BlackboardRoot.LocalToWorld(Vector2.zero);
            var p2 = m_BlackboardRoot.LocalToWorld(localDelta);
            return p2 - p1;
        }

        public Vector2 WorldDeltaToBlackboard(Vector2 worldDelta)
        {
            var p1 = m_BlackboardRoot.WorldToLocal(Vector2.zero);
            var p2 = m_BlackboardRoot.WorldToLocal(worldDelta);
            return p2 - p1;
        }
        #endregion
    }
}