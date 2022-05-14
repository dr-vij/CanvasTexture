using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace ViJ.GraphEditor
{
    public abstract class InputModuleBase
    {
        protected VisualElement m_Target;

        public VisualElement Target
        {
            get => m_Target;
            set
            {
                if (m_Target != null)
                    UnsubscribeEvents(value);
                m_Target = value;
                OnTargetSet(value);
                if (m_Target != null)
                    SubscribeEvents(m_Target);
            }
        }

        public InputModuleBase() { }

        public InputModuleBase(VisualElement target) => Target = target;

        protected abstract void SubscribeEvents(VisualElement eventHandler);
        protected abstract void UnsubscribeEvents(VisualElement eventHandler);

        protected virtual void OnTargetSet(VisualElement target)
        {
        }

        /// <summary>
        /// This method checks if evt current target is captured and stops propagation if needed
        /// </summary>
        /// <param name="evt"></param>
        /// <returns></returns>
        protected virtual bool CanHandleEvent(EventBase evt, bool stopPropagationEvenIfNotCaptured)
        {
            if (m_Target == null)
                return false;

            var isTargetCaptured = evt.currentTarget.HasMouseCapture() && m_Target == evt.currentTarget;
            var canBeHadled = !evt.currentTarget.HasMouseCapture() || isTargetCaptured;
            if (isTargetCaptured || (stopPropagationEvenIfNotCaptured && canBeHadled))
                evt.StopPropagation();
            return canBeHadled;
        }

        /// <summary>
        /// This method checks if evt current event can be handled
        /// </summary>
        /// <param name="evt"></param>
        /// <returns></returns>
        protected virtual bool CanHandleEvent(EventBase evt)
        {
            if (m_Target == null)
                return false;

            return !evt.currentTarget.HasMouseCapture() || m_Target == evt.currentTarget;
        }
    }
}