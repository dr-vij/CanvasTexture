using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public abstract class DataNode<T> : TextureGraphNode where T : NodeSaveData, new()
{
    public DataNode(GraphView graphView) : base(graphView)
    {
    }

    public sealed override void Load(string data)
    {
        var loadData = JsonUtility.FromJson<T>(data);
        Position = loadData.Position;
        OnApplyData(loadData);
    }

    public sealed override string Save() => JsonUtility.ToJson(PrepareSaveData());

    protected T PrepareSaveData()
    {
        var ret = new T()
        {
            NodeType = NodeType,
            Position = Position,
        };
        OnSaveData(ret);
        return ret;
    }

    /// <summary>
    /// Modify data on save
    /// </summary>
    /// <param name="data"></param>
    protected virtual void OnSaveData(T data)
    {
    }

    /// <summary>
    /// Apply data on load
    /// </summary>
    /// <param name="data"></param>
    protected virtual void OnApplyData(T data)
    {
    }
}