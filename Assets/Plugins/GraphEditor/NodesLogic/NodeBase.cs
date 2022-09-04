using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ViJApps
{
    public interface IIdentifieble
    {
        ulong ID { get; }
    }

    public struct ExecutionFlow { }

    public class ExecutionFlowPin : DataPin<ExecutionFlow>
    {
        public ExecutionFlowPin(NodeBase owner) : base(owner)
        {
        }
    }

    public class DataPin<T> : PinBase
    {
        protected T m_Data;

        public T Data
        {
            get => m_Data;
            protected set => m_Data = value;
        }

        public DataPin(NodeBase owner) : base(owner)
        {
        }
    }

    public class PinBase : IIdentifieble
    {
        private Dictionary<ulong, PinBase> m_ConnectedPins = new Dictionary<ulong, PinBase>();
        private NodeBase m_Owner;
        private ulong m_ID;

        public NodeBase Owner => m_Owner;

        public ulong ID => m_ID;

        public event Action<PinBase, PinBase> ConnectionEvent;
        public event Action<PinBase, PinBase> DisconnectionEvent;

        public PinBase(NodeBase owner)
        {
            m_Owner = owner;
        }

        /// <summary>
        /// This method tries to connect pins
        /// </summary>
        /// <param name="otherPin"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public bool TryConnect(PinBase otherPin)
        {
            if (otherPin == null)
                throw new ArgumentNullException("other pin cannot be null");

            if (IsConnectable(otherPin) && !IsConnected(otherPin))
            {
                m_ConnectedPins.Add(otherPin.ID, otherPin);
                otherPin.m_ConnectedPins.Add(ID, this);
                ConnectionEvent?.Invoke(this, otherPin);
                return true;
            }
            return false;
        }

        /// <summary>
        /// This method tries to disconnect pins
        /// </summary>
        /// <param name="otherPin"></param>
        /// <returns></returns>
        public bool TryDisconnect(PinBase otherPin)
        {
            if (otherPin == null)
                throw new ArgumentNullException("other pin cannot be null");

            if (IsConnected(otherPin))
            {
                m_ConnectedPins.Remove(otherPin.ID);
                otherPin.m_ConnectedPins.Remove(ID);
                DisconnectionEvent?.Invoke(this, otherPin);
                return true;
            }
            return false;
        }

        /// <summary>
        /// This method disconnects all input and output pins
        /// </summary>
        public void DisconnectAll()
        {
            var toDisconnectList = m_ConnectedPins.Values.ToArray();
            foreach (var toDisconnect in toDisconnectList)
                TryDisconnect(toDisconnect);
        }

        private bool IsConnected(PinBase otherPin)
        {
            var thisConnectedToOther = m_ConnectedPins.ContainsKey(otherPin.ID);
            var otherConnectedToThis = otherPin.m_ConnectedPins.ContainsKey(ID);
            var bothConnectionsExists = thisConnectedToOther && otherConnectedToThis;
            var anyConnectionExists = thisConnectedToOther || otherConnectedToThis;
            if (bothConnectionsExists != anyConnectionExists)
                throw new Exception("Something wrong with connections");
            return bothConnectionsExists;
        }

        protected virtual bool IsConnectable(PinBase otherPin) => true;
    }

    public class NodeBase: IIdentifieble
    {
        private List<PinBase> m_InputPins = new List<PinBase>();
        private List<PinBase> m_OutputPins = new List<PinBase>();
        private ulong m_ID;

        public ulong ID => m_ID;

        public NodeBase()
        {
        }

        public void DisconnectAll()
        {
            //I convert it to array cause we may have a situation in future,
            //when remove of pin will cause remove off other pin
            var allPins = m_InputPins.Concat(m_OutputPins).ToArray();
            foreach (var pin in allPins)
                pin.DisconnectAll();
        }

        public IReadOnlyList<PinBase> InputPins => m_InputPins;

        public IReadOnlyList<PinBase> OutputPins => m_OutputPins;
    }
}