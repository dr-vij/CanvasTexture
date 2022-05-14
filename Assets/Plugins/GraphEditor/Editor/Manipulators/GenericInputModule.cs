using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace ViJ.GraphEditor
{
    public abstract class GenericInputModule<T> : InputModuleBase where T : VisualElement
    {
        protected T m_TypedTarget;

        public GenericInputModule(T target) : base(target)
        {
        }

        protected sealed override void SubscribeEvents(VisualElement eventHandler)
        {
            OnSubscribeEvents((T)eventHandler);
        }

        protected sealed override void UnsubscribeEvents(VisualElement eventHandler)
        {
            OnUnsubscribeEvents((T)eventHandler);
        }

        protected override void OnTargetSet(VisualElement target)
        {
            m_TypedTarget = (T)target;
        }

        protected abstract void OnSubscribeEvents(T eventHandler);
        protected abstract void OnUnsubscribeEvents(T eventHandler);
    }
}