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
    public class GraphElement : VisualElement
    {
        private const string UXML = nameof(GraphElement) + ".uxml";
        private const string LOCAL_PATH = "Editor/Graph";
        private const string BLACKBOARD_NAME = "BlackboardRoot";
        private const string SELECTIONBOX_NAME = "SelectionBox";

        private VisualElement m_Root;
        private VisualElement m_Background;
        private VisualElement m_BlackboardRoot;
        private VisualElement m_SelectionBox;
        private HashSet<int> m_Buffer = new HashSet<int>();

        private GraphInputModule m_GraphInputModule;

        private int m_NodeIdCounter;
        private Dictionary<int, NodeElement> m_Nodes = new Dictionary<int, NodeElement>();
        private Dictionary<int, NodeInputModule> m_NodeInputs = new Dictionary<int, NodeInputModule>();
        private HashSet<int> mSelectedNodes = new HashSet<int>();
        private HashSet<int> mPreSelectedNodes = new HashSet<int>();

        public event Action<GraphElement> GraphTransformChangeEvent;

        public float ScaleSensetivity { get; set; } = 0.01f;

        public float MoveSensetivity { get; set; } = 3f;

        public Vector2 MinMaxScale { get; set; } = new Vector2(0.01f, 100);

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
            var asset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(Path.Combine(GraphEditorSettings.Instance.PluginPath, LOCAL_PATH, UXML));
            m_Root = asset.Instantiate();
            m_Root.StretchToParentSize();
            Add(m_Root);

            m_BlackboardRoot = m_Root.Q(BLACKBOARD_NAME);
            m_SelectionBox = m_Root.Q(SELECTIONBOX_NAME);

            //Create background
            m_Background = new BackgroundElement(this);
            m_Root.Insert(0, m_Background);

            //Connect Input
            m_GraphInputModule = new GraphInputModule(this);

            m_GraphInputModule.DragStartEvent += OnDragStart;
            m_GraphInputModule.DragEvent += OnDrag;
            m_GraphInputModule.DragEndEvent += OnDragEnd;

            m_GraphInputModule.RectSelectionStartEvent += OnRectSelectionStart;
            m_GraphInputModule.RectSelectionEvent += OnRectSelection;
            m_GraphInputModule.RectSelectionEndEvent += OnRectSelectionEnd;

            m_GraphInputModule.ZoomEvent += OnZoom;
            m_GraphInputModule.SlideEvent += OnSlide;
        }

        public void AddNode(NodeElement node)
        {
            //Create node and add it to blackboard
            node.ID = GetNextId();
            m_Nodes.Add(node.ID, node);
            m_BlackboardRoot.Add(node);

            //Create input for node and subscribe it
            var inputModule = new NodeInputModule(node);
            inputModule.NodeDragStartEvent += OnNodeDragStart;
            inputModule.NodeDragEvent += OnNodeDrag;
            inputModule.NodeDragEndEvent += OnNodeDragEnd;
            m_NodeInputs.Add(node.ID, inputModule);
        }

        public void RemoveNode(int id)
        {
            //Remove node from blackboard
            var node = m_Nodes[id];
            m_Nodes.Remove(id);
            mSelectedNodes.Remove(id);
            mPreSelectedNodes.Remove(id);
            node.RemoveFromHierarchy();

            //Disconnect input
            var inputModule = m_NodeInputs[id];
            inputModule.NodeDragStartEvent -= OnNodeDragStart;
            inputModule.NodeDragEvent -= OnNodeDrag;
            inputModule.NodeDragEndEvent -= OnNodeDragEnd;
            inputModule.Dispose();
            m_NodeInputs.Remove(id);
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

            foreach (var nodeId in nodesToPreselect)
            {
                m_Nodes[nodeId].IsPreSelected = true;
                mPreSelectedNodes.Add(nodeId);
            }
            foreach (var nodeId in nodesToUnselect)
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

        #region Nodes drag

        private Vector2 m_PointerDragStartPosition;
        private Vector2 m_NodeDragStartPosition;

        private void OnNodeDragStart(NodeElement node, Vector2 pointerPosition)
        {
            //TODO: Select if not selected. Move group if group selected

            m_PointerDragStartPosition = pointerPosition;
            m_NodeDragStartPosition = node.BlackboardPosition;
        }

        private void OnNodeDrag(NodeElement node, Vector2 pointerPosition)
        {
            var pointerTotalDelta = pointerPosition - m_PointerDragStartPosition;
            var blackboardDelta = WorldDeltaToBlackboard(pointerTotalDelta);
            node.BlackboardPosition = m_NodeDragStartPosition + blackboardDelta;
        }

        private void OnNodeDragEnd(NodeElement node, Vector2 pointerPosition)
        {
        }

        #endregion

        #region Graph manipulations

        private Vector2 m_GraphPointerDragStartPos;
        private Vector2 m_SelectorRectDragStartPos;

        private void OnDragStart(Vector2 pointerPosition)
        {
            m_GraphPointerDragStartPos = WorldPointToBlackboard(pointerPosition);
        }

        private void OnDrag(Vector2 pointerPosition)
        {
            var mouseNewPosition = WorldPointToBlackboard(pointerPosition);
            var localDelta = mouseNewPosition - m_GraphPointerDragStartPos;
            var delta = BlackboardDeltaToWorld(localDelta);
            Position += delta;
        }

        private void OnDragEnd(Vector2 pointerPosition)
        {
        }

        private void OnRectSelectionStart(Vector2 pointerPosition)
        {
            m_SelectorRectDragStartPos = WorldPointToBlackboard(pointerPosition);
            var fromTo = WorldPointToRoot(pointerPosition);
            StartSelectionBox(fromTo, fromTo);
        }

        private void OnRectSelection(Vector2 pointerPosition)
        {
            var from = BlackboardPointToRoot(m_SelectorRectDragStartPos);
            var to = WorldPointToRoot(pointerPosition);
            UpdateSelectionBox(from, to);
        }

        private void OnRectSelectionEnd(Vector2 pointerPosition)
        {
            var from = BlackboardPointToRoot(m_SelectorRectDragStartPos);
            var to = WorldPointToRoot(pointerPosition);
            EndSelectionBox(from, to);
        }

        private void OnSlide(Vector2 delta)
        {
            Position += delta * MoveSensetivity;
        }

        private void StartSelectionBox(Vector2 from, Vector2 to)
        {
            m_SelectionBox.pickingMode = PickingMode.Ignore;
            m_SelectionBox.style.visibility = Visibility.Visible;
            m_SelectionBox.style.opacity = 1;
            var rect = UpdateVisualBox(from, to);
            m_Buffer = GetOverlappedNodes(rect, m_Buffer);
            SetPreselectedNodes(m_Buffer);
        }

        private void UpdateSelectionBox(Vector2 from, Vector2 to)
        {
            var rect = UpdateVisualBox(from, to);
            m_Buffer = GetOverlappedNodes(rect, m_Buffer);
            SetPreselectedNodes(m_Buffer);
        }

        private void EndSelectionBox(Vector2 from, Vector2 to)
        {
            var rect = UpdateVisualBox(from, to);
            m_SelectionBox.style.opacity = 0;
            SetPreselectedNodes(null);
            m_Buffer = GetOverlappedNodes(rect, m_Buffer);
            SetSelectedNodes(m_Buffer);
        }

        /// <summary>
        /// Scales graph relative to the given world position
        /// </summary>
        /// <param name="pointerWorldPos"></param>
        /// <param name="scaleDelta"></param>
        private void OnZoom(Vector2 worldPosition, float scaleDelta)
        {
            //Save position of pointer before scale
            var mousePosBefore = WorldPointToRoot(worldPosition);
            var mouseLocalPosBefore = WorldPointToBlackboard(worldPosition);

            //Scale
            var currentScale = Scale;
            var delta = Mathf.Clamp(1 + ScaleSensetivity * scaleDelta, 0.5f, 1.5f);
            var wantedScale = currentScale * delta;
            var clampedScale = Mathf.Clamp(wantedScale, MinMaxScale.x, MinMaxScale.y);
            Scale = clampedScale;

            //Now reposition
            var mousePosAfter = BlackboardPointToRoot(mouseLocalPosBefore);
            var moveDelta = mousePosAfter - mousePosBefore;
            Position -= moveDelta;
        }


        #endregion

        #region Coords conversions

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