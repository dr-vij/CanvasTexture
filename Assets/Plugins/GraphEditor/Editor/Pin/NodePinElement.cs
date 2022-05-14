using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace ViJ.GraphEditor
{
    public delegate void PinDragHandler(NodePinElement pin, Vector2 position);

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

        public event PinDragHandler PinDragStartEvent;
        public event PinDragHandler PinDragEvent;
        public event PinDragHandler PinDragEndEvent;

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
                if (m_Owner == null)
                {
                    if (value != null)
                        m_Owner = value;
                    else
                        throw new System.Exception("Owner of pin must not be set null");
                }
                else
                {
                    throw new System.Exception("Owner of pin must not be changed");
                }
            }
        }

        public NodePinElement()
        {
            m_Pin = this.Q(className: PIN_CLASSNAME);
            m_PinContainer = this.Q(className: PIN_CONTAINER_CLASS);

            //Connect input
            m_DragInputModule = new DragInputModule(this);
            m_DragInputModule.NodeDragStartEvent += (position) => PinDragStartEvent?.Invoke(this, position);
            m_DragInputModule.NodeDragEvent += (position) => PinDragEvent?.Invoke(this, position);
            m_DragInputModule.NodeDragEndEvent += (position) => PinDragEndEvent?.Invoke(this, position);
        }
    }
}