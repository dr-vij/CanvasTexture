using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ViJ.GraphEditor
{
    /// <summary>
    /// Base graph element
    /// </summary>
    public class GraphElement : VisualElement
    {
        private const string UXML = nameof(GraphElement) + ".uxml";
        private const string ELEMENT_PATH = "Editor";
        private const string BLACKBOARD_NAME = "BlackboardRoot";
        private const string SELECTIONBOX_NAME = "SelectionBox";

        public new class UxmlFactory : UxmlFactory<GraphElement, UxmlTraits> { }
        public new class UxmlTraits : VisualElement.UxmlTraits { }

        private VisualElement m_Root;
        private VisualElement m_Background;
        private VisualElement m_BlackboardRoot;
        private VisualElement m_SelectionBox;

        private int m_NodeIdCounter;
        private Dictionary<int, GraphNodeElement> mNodes = new Dictionary<int, GraphNodeElement>();
        private HashSet<int> mSelectedNodes = new HashSet<int>();
        private HashSet<int> mPreSelectedNodes = new HashSet<int>();

        public event Action<GraphElement> GraphTransformChangeEvent;

        public Vector2 Position
        {
            get => m_BlackboardRoot.transform.position;
            set
            {
                m_BlackboardRoot.transform.position = value;
                GraphTransformChangeEvent?.Invoke(this);
            }
        }

        public float Scale
        {
            get => m_BlackboardRoot.transform.scale.x;
            set
            {
                m_BlackboardRoot.transform.scale = new Vector2(value, value);
                GraphTransformChangeEvent?.Invoke(this);
            }
        }

        public GraphElement()
        {
            var asset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(Path.Combine(TextureGraphSettings.PLUGIN_PATH, ELEMENT_PATH, UXML));
            m_Root = asset.Instantiate();
            m_Root.StretchToParentSize();
            Add(m_Root);

            m_BlackboardRoot = m_Root.Q(BLACKBOARD_NAME);
            m_SelectionBox = m_Root.Q(SELECTIONBOX_NAME);

            //Create background
            m_Background = new BackgroundElement(this);
            m_Root.Insert(0, m_Background);
        }

        public void AddToBlackboard(VisualElement element)
        {
            m_BlackboardRoot.Add(element);
        }

        public void AddNode(GraphNodeElement node)
        {
            node.ID = GetNextId();
            mNodes.Add(node.ID, node);

            m_BlackboardRoot.Add(node);
        }

        public void RemoveNode(int id)
        {
            var node = mNodes[id];
            mNodes.Remove(id);
            mSelectedNodes.Remove(id);
            mPreSelectedNodes.Remove(id);
            node.RemoveFromHierarchy();
        }

        public void StartSelectionBox(Vector2 from, Vector2 to)
        {
            m_SelectionBox.pickingMode = PickingMode.Ignore;
            m_SelectionBox.style.visibility = Visibility.Visible;
            m_SelectionBox.style.opacity = 1;
            var rect = UpdateVisualBox(from, to);
        }

        public void UpdateSelectionBox(Vector2 from, Vector2 to)
        {
            var rect = UpdateVisualBox(from, to);
        }

        public void EndSelectionBox(Vector2 from, Vector2 to)
        {
            var rect = UpdateVisualBox(from, to);
            m_SelectionBox.style.opacity = 0;
        }

        private IList<GraphNodeElement> GetOverlappedNodes(Rect rect)
        {
            var ret = new List<GraphNodeElement>();
            foreach(var node in mNodes.Values)
            {
                if (node.worldBound.Overlaps(rect))
                    ret.Add(node);
            }
            return ret;
        }

        private Rect UpdateVisualBox(Vector2 from, Vector2 to)
        {
            var fromCorrected = Vector2.Min(from, to);
            var toCorrected = Vector2.Max(from, to);
            m_SelectionBox.transform.position = fromCorrected;
            m_SelectionBox.style.width = Mathf.Abs(toCorrected.x - fromCorrected.x);
            m_SelectionBox.style.height = Mathf.Abs(toCorrected.y - fromCorrected.y);
            return m_SelectionBox.worldBound;
        }

        #region Coords conversion

        //POINT
        public Vector2 BlackboardPointToWorld(Vector2 localPosition) => m_BlackboardRoot.LocalToWorld(localPosition);

        //POINT
        public Vector2 WorldPointToBlackboard(Vector2 worldPosition) => m_BlackboardRoot.WorldToLocal(worldPosition);

        //POINT
        public Vector2 WorldPointToRoot(Vector2 worldPosition) => m_Root.WorldToLocal(worldPosition);

        //POINT
        public Vector2 RootPointToWorld(Vector2 localPosition) => m_Root.LocalToWorld(localPosition);

        //POINT
        public Vector2 BlackboardPointToRoot(Vector2 localPosition)
        {
            var worldPos = m_BlackboardRoot.LocalToWorld(localPosition);
            return m_Root.WorldToLocal(worldPos);
        }

        //POINT
        public Vector2 RootPointToBlackboard(Vector2 localPosition)
        {
            var worldPos = m_Root.LocalToWorld(localPosition);
            return m_BlackboardRoot.WorldToLocal(worldPos);
        }

        //DELTA
        public Vector2 BlackboardDeltaToRoot(Vector2 localDelta)
        {
            var p1 = m_Root.WorldToLocal(m_BlackboardRoot.LocalToWorld(Vector2.zero));
            var p2 = m_Root.WorldToLocal(m_BlackboardRoot.LocalToWorld(localDelta));
            return p2 - p1;
        }

        //DELTA
        public Vector2 BlackboardDeltaToWorld(Vector2 localDelta)
        {
            var p1 = m_BlackboardRoot.LocalToWorld(Vector2.zero);
            var p2 = m_BlackboardRoot.LocalToWorld(localDelta);
            return p2 - p1;
        }

        //DELTA
        public Vector2 WorldDeltaToBlackboard(Vector2 worldDelta)
        {
            var p1 = m_BlackboardRoot.WorldToLocal(Vector2.zero);
            var p2 = m_BlackboardRoot.WorldToLocal(worldDelta);
            return p2 - p1;
        }

        #endregion

        private int GetNextId()
        {
            return m_NodeIdCounter++;
        }
    }
}