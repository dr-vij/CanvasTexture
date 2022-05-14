using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.IO;
using System;

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

        public event Action NodePositionChangeEvent;

        public event NodeDrag NodeDragStartEvent;
        public event NodeDrag NodeDragEvent;
        public event NodeDrag NodeDragEndEvent;

        public Rect NodeWorldBounds => m_Node.worldBound;

        public Vector2 BlackboardPosition
        {
            get => transform.position;
            set
            {
                transform.position = value;
                NodePositionChangeEvent?.Invoke();
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

        public NodeElement()
        {
            var asset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(Path.Combine(GraphEditorSettings.Instance.PluginPath, LOCAL_PATH, UXML));
            Add(asset.Instantiate());
            m_Node = this.Q<VisualElement>("Node");
            UpdateView();

            //Connect input;
            m_DragInputModule = new DragInputModule(this);
            m_DragInputModule.NodeDragStartEvent += (coord) => NodeDragStartEvent?.Invoke(this, coord);
            m_DragInputModule.NodeDragEvent += (coord) => NodeDragEvent?.Invoke(this, coord);
            m_DragInputModule.NodeDragEndEvent += (coord) => NodeDragEndEvent?.Invoke(this, coord);
        }

        public void AddOutputPin(NodePinElement pin)
        {
            m_OutputContainer.Add(pin);
            pin.IsReversed = false;
        }

        public void AddInputPin(NodePinElement pin)
        {
            m_InputContainer.Add(pin);
            pin.IsReversed = true;
        }

        private void AddPin(NodePinElement pin)
        {
            //pin.PinDragStartEvent += 
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