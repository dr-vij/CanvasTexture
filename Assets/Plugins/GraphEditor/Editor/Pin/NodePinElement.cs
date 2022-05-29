using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ViJ.GraphEditor
{
    public delegate void PinDrag(NodePinElement pin, Vector2 position);

    public enum PinType
    {
        None,
        Input,
        Output,
    }

    public class NodePinElement : IdentifiedVisualElement
    {
        private const string UXML = nameof(NodePinElement) + ".uxml";
        private const string LOCAL_PATH = "Editor/Pin";
        private const string PIN_CLASSNAME = "pin";
        private const string PIN_CONTAINER_CLASS = "pinContainer";
        private const string PIN_CONTAINER_REVERSED_CLASS = "pinContainerReversed";

        private DragInputModule m_DragInputModule;
        private NodeElement m_Owner;
        private VisualElement m_Pin;
        private VisualElement m_PinContainer;
        private bool m_IsReversed;
        private PinType m_PinType = PinType.None;

        public event PinDrag PinDragStartEvent;
        public event PinDrag PinDragEvent;
        public event PinDrag PinDragEndEvent;

        public event Action PinPositionChangeEvent;

        public Vector2 PinWorldPosition => m_Pin.LocalToWorld(m_Pin.contentRect.center);

        public PinType PinType
        {
            get => m_PinType;
            set => m_PinType = value;
        }

        public bool IsReversed
        {
            get => m_IsReversed;
            set
            {
                m_IsReversed = value;
                if (m_IsReversed && !m_PinContainer.ClassListContains(PIN_CONTAINER_REVERSED_CLASS))
                    m_PinContainer.AddToClassList(PIN_CONTAINER_REVERSED_CLASS);
                else if (!m_IsReversed && m_PinContainer.ClassListContains(PIN_CONTAINER_REVERSED_CLASS))
                    m_PinContainer.RemoveFromClassList(PIN_CONTAINER_REVERSED_CLASS);
            }
        }

        public NodeElement Owner
        {
            get => m_Owner;
            set
            {
                m_Owner = value;
                m_Owner.NodeTransformChangeEvent += () => PinPositionChangeEvent?.Invoke();
            }
        }

        public NodePinElement()
        {
            var asset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(Path.Combine(GraphEditorSettings.Instance.PluginPath, LOCAL_PATH, UXML));
            Add(asset.Instantiate());
            m_Pin = this.Q(className: PIN_CLASSNAME);
            m_PinContainer = this.Q(className: PIN_CONTAINER_CLASS);

            generateVisualContent += (c) => PinPositionChangeEvent?.Invoke();

            //Connect input
            m_DragInputModule = new DragInputModule(this);
            m_DragInputModule.NodeDragStartEvent += (position) => PinDragStartEvent?.Invoke(this, position);
            m_DragInputModule.NodeDragEvent += (position) => PinDragEvent?.Invoke(this, position);
            m_DragInputModule.NodeDragEndEvent += (position) => PinDragEndEvent?.Invoke(this, position);
        }
    }
}