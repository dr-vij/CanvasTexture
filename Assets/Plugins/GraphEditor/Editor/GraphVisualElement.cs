using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ViJ.GraphEditor
{
    /// <summary>
    /// Base graph element
    /// </summary>
    public class GraphVisualElement : VisualElement
    {
        private const string UXML = nameof(GraphVisualElement) + ".uxml";
        private const string ELEMENT_PATH = "Editor";
        private const string BLACKBOARD_NAME = "BlackboardRoot";
        private const string SELECTIONBOX_NAME = "SelectionBox";

        public new class UxmlFactory : UxmlFactory<GraphVisualElement, UxmlTraits> { }
        public new class UxmlTraits : VisualElement.UxmlTraits { }

        private VisualElement m_Root;
        private VisualElement m_Background;
        private VisualElement m_BlackboardRoot;
        private VisualElement m_SelectionBox;
        private HashSet<int> m_Buffer = new HashSet<int>();

        private int m_NodeIdCounter;
        private Dictionary<int, GraphNodeElement> m_Nodes = new Dictionary<int, GraphNodeElement>();
        private HashSet<int> mSelectedNodes = new HashSet<int>();
        private HashSet<int> mPreSelectedNodes = new HashSet<int>();

        public event Action<GraphVisualElement> GraphTransformChangeEvent;

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

        public GraphVisualElement()
        {
            var asset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(Path.Combine(GraphEditorSettings.Instance.PluginPath, ELEMENT_PATH, UXML));
            m_Root = asset.Instantiate();
            m_Root.StretchToParentSize();
            Add(m_Root);

            m_BlackboardRoot = m_Root.Q(BLACKBOARD_NAME);
            m_SelectionBox = m_Root.Q(SELECTIONBOX_NAME);

            //Create background
            m_Background = new BackgroundElement(this);
            m_Root.Insert(0, m_Background);
        }

        public void AddNode(GraphNodeElement node)
        {
            node.ID = GetNextId();
            m_Nodes.Add(node.ID, node);

            m_BlackboardRoot.Add(node);
        }

        public void RemoveNode(int id)
        {
            var node = m_Nodes[id];
            m_Nodes.Remove(id);
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
            m_Buffer = GetOverlappedNodes(rect, m_Buffer);
            SetPreselectedNodes(m_Buffer);
        }

        public void UpdateSelectionBox(Vector2 from, Vector2 to)
        {
            var rect = UpdateVisualBox(from, to);
            m_Buffer = GetOverlappedNodes(rect, m_Buffer);
            SetPreselectedNodes(m_Buffer);
        }

        public void EndSelectionBox(Vector2 from, Vector2 to)
        {
            var rect = UpdateVisualBox(from, to);
            m_SelectionBox.style.opacity = 0;
            SetPreselectedNodes(null);
            m_Buffer = GetOverlappedNodes(rect, m_Buffer);
            SetSelectedNodes(m_Buffer);
        }

        private HashSet<int> GetOverlappedNodes(Rect rect, HashSet<int> outNodes = null)
        {
            if (outNodes == null)
                outNodes = new HashSet<int>();
            else
                outNodes.Clear();

            foreach (var node in m_Nodes.Values)
            {
                if (node.NodeWorldBounds.Overlaps(rect))
                    outNodes.Add(node.ID);
            }
            return outNodes;
        }

        private void SetPreselectedNodes(HashSet<int> nodes)
        {
            if (nodes == null)
                nodes = new HashSet<int>();

            var nodesToPreselect = nodes.Where(c => !mPreSelectedNodes.Contains(c)).ToList();
            var nodesToUnselect = mPreSelectedNodes.Where(c => !nodes.Contains(c)).ToList();

            foreach(var nodeId in nodesToPreselect)
            {
                m_Nodes[nodeId].IsPreSelected = true;
                mPreSelectedNodes.Add(nodeId);
            }
            foreach(var nodeId in nodesToUnselect)
            {
                m_Nodes[nodeId].IsPreSelected = false;
                mPreSelectedNodes.Remove(nodeId);
            }    
        }

        private void SetSelectedNodes(HashSet<int> nodes)
        {
            var nodesToSelect = nodes.Where(c => !mSelectedNodes.Contains(c)).ToList();
            var nodesToUnselect = mSelectedNodes.Where(c => !nodes.Contains(c)).ToList();

            foreach (var nodeId in nodesToSelect)
            {
                m_Nodes[nodeId].IsSelected = true;
                mSelectedNodes.Add(nodeId);
            }
            foreach (var nodeId in nodesToUnselect)
            {
                m_Nodes[nodeId].IsSelected = false;
                mSelectedNodes.Remove(nodeId);
            }
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