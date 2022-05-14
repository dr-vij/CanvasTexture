using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace ViJ.GraphEditor
{
    public abstract class InputModuleBase : IDisposable
    {
        private bool m_IsDisposed = false;
        protected VisualElement m_Target;

        public InputModuleBase(VisualElement target)
        {
            m_Target = target;
            OnTargetSet(target);
            SubscribeEvents(target);
        }

        protected abstract void OnTargetSet(VisualElement target);
        protected abstract void SubscribeEvents(VisualElement eventHandler);
        protected abstract void UnsubscribeEvents(VisualElement eventHandler);

        /// <summary>
        /// This method checks if evt current target is captured and stops propagation if needed
        /// </summary>
        /// <param name="evt"></param>
        /// <returns></returns>
        protected virtual bool CanHandleEvent(EventBase evt, bool stopPropagationEvenIfNotCaptured)
        {
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
            return !evt.currentTarget.HasMouseCapture() || m_Target == evt.currentTarget;
        }

        public void Dispose()
        {
            if (!m_IsDisposed)
            {
                UnsubscribeEvents(m_Target);
                OnDispose();
                m_IsDisposed = true;
            }
        }

        protected virtual void OnDispose()
        {
        }
    }
}