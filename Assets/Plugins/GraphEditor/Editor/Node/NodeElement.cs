using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.IO;
using System;
using System.Linq;

namespace ViJ.GraphEditor
{
    public delegate void NodeDrag(NodeElement node, Vector2 position);

    public class IdentifiedVisualElement : VisualElement
    {
        private int m_ID = -1;

        public int ID
        {
            get => m_ID;
            set
            {
                if (m_ID == -1)
                {
                    if (value >= 0)
                        m_ID = value;
                    else
                        throw new Exception("Incorrect ID");
                }
                else
                {
                    throw new Exception("ID must not be changed");
                }
            }
        }

        public bool HasId => m_ID != -1;
    }

    public class NodeElement : IdentifiedVisualElement
    {
        private const string UXML = nameof(NodeElement) + ".uxml";
        private const string LOCAL_PATH = "Editor/Node";
        private const string SELECTED_NODE_CLASS = "nodeSelected";
        private const string PRESELECTED_NODE_CLASS = "nodePreselected";
        private const string UNSELECTED_NODE_CLASS = "nodeUnselected";

        private const string INPUT_CONTAINER_CLASS = "inputContainer";
        private const string OUTPUT_CONTAINER_CLASS = "outputContainer";

        private bool mIsSelected = false;
        private bool mIsPreselected = false;
        private VisualElement m_Node;
        private VisualElement m_InputContainer;
        private VisualElement m_OutputContainer;
        private DragInputModule m_DragInputModule;

        private GraphElement m_Owner;

        private int m_PinsCounter = -1;
        private Dictionary<int, NodePinElement> m_Pins = new Dictionary<int, NodePinElement>();

        public event Action NodeTransformChangeEvent;

        public event NodeDrag NodeDragStartEvent;
        public event NodeDrag NodeDragEvent;
        public event NodeDrag NodeDragEndEvent;

        public event PinDrag PinDragStartEvent;
        public event PinDrag PinDragEvent;
        public event PinDrag PinDragEndEvent;

        public Rect NodeWorldBounds => m_Node.worldBound;

        public Vector2 BlackboardPosition
        {
            get => transform.position;
            set
            {
                transform.position = value;
                NodeTransformChangeEvent?.Invoke();
            }
        }

        public bool IsSelected
        {
            get => mIsSelected;
            set
            {
                if (mIsSelected != value)
                {
                    mIsSelected = value;
                    UpdateView();
                }
            }
        }

        public bool IsPreSelected
        {
            get => mIsPreselected;
            set
            {
                if (mIsPreselected != value)
                {
                    mIsPreselected = value;
                    UpdateView();
                }
            }
        }

        public NodeElement(GraphElement graphElement)
        {
            m_Owner = graphElement;
            m_Owner.AddNode(this);

            var asset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(Path.Combine(GraphEditorSettings.Instance.PluginPath, LOCAL_PATH, UXML));
            Add(asset.Instantiate());
            m_Node = this.Q("Node");
            m_InputContainer = this.Q(className: INPUT_CONTAINER_CLASS);
            m_OutputContainer = this.Q(className: OUTPUT_CONTAINER_CLASS);
            UpdateView();

            //Connect input;
            m_DragInputModule = new DragInputModule(this);
            m_DragInputModule.NodeDragStartEvent += (coord) => NodeDragStartEvent?.Invoke(this, coord);
            m_DragInputModule.NodeDragEvent += (coord) => NodeDragEvent?.Invoke(this, coord);
            m_DragInputModule.NodeDragEndEvent += (coord) => NodeDragEndEvent?.Invoke(this, coord);

            //TODO: REMOVE TEST PINS
            AddOutputPin(new NodePinElement());
            AddOutputPin(new NodePinElement());
            AddOutputPin(new NodePinElement());
            AddOutputPin(new NodePinElement());

            AddInputPin(new NodePinElement());
            AddInputPin(new NodePinElement());
        }

        public List<NodePinElement> GetAllPins() => m_Pins.Values.ToList();

        public bool TryGetPin(int pinId, out NodePinElement pin) => m_Pins.TryGetValue(pinId, out pin);

        public int AddOutputPin(NodePinElement pin)
        {
            m_OutputContainer.Add(pin);
            pin.IsReversed = false;
            pin.PinType = PinType.Output;
            var id =  RegisterPin(pin);
            NodeTransformChangeEvent?.Invoke();
            return id;
        }

        public int AddInputPin(NodePinElement pin)
        {
            m_InputContainer.Add(pin);
            pin.IsReversed = true;
            pin.PinType = PinType.Input;
            var id = RegisterPin(pin);
            NodeTransformChangeEvent?.Invoke();
            return id;
        }

        private int RegisterPin(NodePinElement pin)
        {
            if (!pin.HasId)
                pin.ID = ++m_PinsCounter;

            pin.Owner = this;
            pin.PinDragStartEvent += (pin, coord) => PinDragStartEvent?.Invoke(pin, coord);
            pin.PinDragEvent += (pin, coord) => PinDragEvent?.Invoke(pin, coord);
            pin.PinDragEndEvent += (pin, coord) => PinDragEndEvent?.Invoke(pin, coord);

            m_Pins.Add(pin.ID, pin);
            return pin.ID;
        }

        /// <summary>
        /// Update styles
        /// </summary>
        private void UpdateView()
        {
            //node has unselected style if it is not selected or preselected
            if (!mIsSelected && !mIsPreselected)
                m_Node.AddToClassList(UNSELECTED_NODE_CLASS);
            else if (mIsSelected || mIsPreselected)
                m_Node.RemoveFromClassList(UNSELECTED_NODE_CLASS);

            //preselected has higher priority
            if (mIsPreselected)
                m_Node.AddToClassList(PRESELECTED_NODE_CLASS);
            else
                m_Node.RemoveFromClassList(PRESELECTED_NODE_CLASS);

            //selected has lowest priority
            if (mIsSelected && !mIsPreselected)
                m_Node.AddToClassList(SELECTED_NODE_CLASS);
            else
                m_Node.RemoveFromClassList(SELECTED_NODE_CLASS);
        }
    }
}