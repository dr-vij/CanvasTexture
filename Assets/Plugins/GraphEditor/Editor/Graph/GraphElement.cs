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
        private HashSet<int> m_SelectedNodes = new HashSet<int>();
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

        public void ConnectPins(NodePinElement pin0, NodePinElement pin1)
        {
            var connection = new ConnectionElement();
            m_BlackboardRoot.Add(connection);
            connection.SetPins(pin0, pin1);
        }

        public void AddNode(NodeElement node)
        {
            //Create node and add it to blackboard
            node.ID = GetNextId();
            m_Nodes.Add(node.ID, node);
            m_BlackboardRoot.Add(node);

            //Connect input
            node.NodeDragStartEvent += OnNodeDragStart;
            node.NodeDragEvent += OnNodeDrag;
            node.NodeDragEndEvent += OnNodeDragEnd;
        }

        public void RemoveNode(int id)
        {
            //Remove node from blackboard
            var node = m_Nodes[id];
            m_Nodes.Remove(id);
            m_SelectedNodes.Remove(id);
            mPreSelectedNodes.Remove(id);
            node.RemoveFromHierarchy();

            //Disconnect input
            node.NodeDragStartEvent -= OnNodeDragStart;
            node.NodeDragEvent -= OnNodeDrag;
            node.NodeDragEndEvent -= OnNodeDragEnd;
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

        private void PreselectNodes(HashSet<int> nodes)
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

        private void SelectNodes(HashSet<int> nodes)
        {
            var nodesToSelect = nodes.Where(c => !m_SelectedNodes.Contains(c)).ToList();
            var nodesToUnselect = m_SelectedNodes.Where(c => !nodes.Contains(c)).ToList();

            foreach (var nodeId in nodesToSelect)
            {
                m_Nodes[nodeId].IsSelected = true;
                m_SelectedNodes.Add(nodeId);
            }
            foreach (var nodeId in nodesToUnselect)
            {
                m_Nodes[nodeId].IsSelected = false;
                m_SelectedNodes.Remove(nodeId);
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

        #region Pin manipulations

        private void OnPinDragStart(NodePinElement pin, Vector2 position)
        {

        }

        private void OnPinDrag(NodePinElement pin, Vector2 position)
        {

        }

        private void OnPinDragEnd(NodePinElement pin, Vector2 position)
        {

        }

        #endregion

        #region Nodes manipulations

        private Vector2 m_PointerDragStartPosition;
        private Dictionary<int, Vector2> m_DragStartPositions = new Dictionary<int, Vector2>();

        private void OnNodeDragStart(NodeElement node, Vector2 pointerPosition)
        {
            m_DragStartPositions.Clear();

            //TODO: Select if not selected. Move group if group selected
            if (!m_SelectedNodes.Contains(node.ID))
                SelectNodes(new HashSet<int>(new[] { node.ID }));

            foreach (var selectedId in m_SelectedNodes)
                m_DragStartPositions[selectedId] = m_Nodes[selectedId].BlackboardPosition;

            m_PointerDragStartPosition = pointerPosition;
        }

        private void OnNodeDrag(NodeElement node, Vector2 pointerPosition)
        {
            var pointerTotalDelta = pointerPosition - m_PointerDragStartPosition;
            var blackboardDelta = TransformDeltaWorldToBlackboard(pointerTotalDelta);

            foreach (var selectedId in m_SelectedNodes)
                m_Nodes[selectedId].BlackboardPosition = m_DragStartPositions[selectedId] + blackboardDelta;
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
            m_GraphPointerDragStartPos = TransformPositionWorldToBlackboard(pointerPosition);
        }

        private void OnDrag(Vector2 pointerPosition)
        {
            var mouseNewPosition = TransformPositionWorldToBlackboard(pointerPosition);
            var localDelta = mouseNewPosition - m_GraphPointerDragStartPos;
            var delta = TransformDeltaBlackboardToWorld(localDelta);
            Position += delta;
        }

        private void OnDragEnd(Vector2 pointerPosition)
        {
        }

        private void OnRectSelectionStart(Vector2 pointerPosition)
        {
            m_SelectorRectDragStartPos = TransformPositionWorldToBlackboard(pointerPosition);
            var fromTo = TransformPositionWorldToRoot(pointerPosition);
            StartSelectionBox(fromTo, fromTo);
        }

        private void OnRectSelection(Vector2 pointerPosition)
        {
            var from = TransformPositionBlackboardToRoot(m_SelectorRectDragStartPos);
            var to = TransformPositionWorldToRoot(pointerPosition);
            UpdateSelectionBox(from, to);
        }

        private void OnRectSelectionEnd(Vector2 pointerPosition)
        {
            var from = TransformPositionBlackboardToRoot(m_SelectorRectDragStartPos);
            var to = TransformPositionWorldToRoot(pointerPosition);
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
            PreselectNodes(m_Buffer);
        }

        private void UpdateSelectionBox(Vector2 from, Vector2 to)
        {
            var rect = UpdateVisualBox(from, to);
            m_Buffer = GetOverlappedNodes(rect, m_Buffer);
            PreselectNodes(m_Buffer);
        }

        private void EndSelectionBox(Vector2 from, Vector2 to)
        {
            var rect = UpdateVisualBox(from, to);
            m_SelectionBox.style.opacity = 0;
            PreselectNodes(null);
            m_Buffer = GetOverlappedNodes(rect, m_Buffer);
            SelectNodes(m_Buffer);
        }

        /// <summary>
        /// Scales graph relative to the given world position
        /// </summary>
        /// <param name="pointerWorldPos"></param>
        /// <param name="scaleDelta"></param>
        private void OnZoom(Vector2 worldPosition, float scaleDelta)
        {
            //Save position of pointer before scale
            var mousePosBefore = TransformPositionWorldToRoot(worldPosition);
            var mouseLocalPosBefore = TransformPositionWorldToBlackboard(worldPosition);

            //Scale
            var currentScale = Scale;
            var delta = Mathf.Clamp(1 + ScaleSensetivity * scaleDelta, 0.5f, 1.5f);
            var wantedScale = currentScale * delta;
            var clampedScale = Mathf.Clamp(wantedScale, MinMaxScale.x, MinMaxScale.y);
            Scale = clampedScale;

            //Now reposition
            var mousePosAfter = TransformPositionBlackboardToRoot(mouseLocalPosBefore);
            var moveDelta = mousePosAfter - mousePosBefore;
            Position -= moveDelta;
        }

        #endregion

        #region Coords conversions

        public Vector2 TransformPositionBlackboardToWorld(Vector2 localPosition) => m_BlackboardRoot.LocalToWorld(localPosition);
        public Vector2 TransformPositionWorldToBlackboard(Vector2 worldPosition) => m_BlackboardRoot.WorldToLocal(worldPosition);

        public Vector2 TransformPositionWorldToRoot(Vector2 worldPosition) => m_Root.WorldToLocal(worldPosition);
        public Vector2 TransformPositionRootToWorld(Vector2 localPosition) => m_Root.LocalToWorld(localPosition);

        public Vector2 TransformPositionBlackboardToRoot(Vector2 localPosition) => m_BlackboardRoot.ChangeCoordinatesTo(m_Root, localPosition);
        public Vector2 TransformPositionRootToBlackboard(Vector2 localPosition) => m_Root.ChangeCoordinatesTo(m_BlackboardRoot, localPosition);

        public Vector2 TransformDeltaBlackboardToRoot(Vector2 localDelta) => m_BlackboardRoot.ChangeCoordinatesTo(m_Root, localDelta) - m_BlackboardRoot.ChangeCoordinatesTo(m_Root, Vector2.zero);
        public Vector2 TransformDeltaBlackboardToWorld(Vector2 localDelta) => m_BlackboardRoot.LocalToWorld(localDelta) - m_BlackboardRoot.LocalToWorld(Vector2.zero);
        public Vector2 TransformDeltaWorldToBlackboard(Vector2 worldDelta) => m_BlackboardRoot.WorldToLocal(worldDelta) - m_BlackboardRoot.WorldToLocal(Vector2.zero);

        #endregion

        private int GetNextId() => m_NodeIdCounter++;
    }
}