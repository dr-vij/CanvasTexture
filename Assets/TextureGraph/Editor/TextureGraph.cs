using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextureGraph : ScriptableObject
{
    [SerializeField] private List<string> m_SaveData = new List<string>();

    public void Save(IEnumerable<NodeSaveData> saveData)
    {
        m_SaveData.Clear();

        foreach (var data in saveData)
        {
            m_SaveData.Add(JsonUtility.ToJson(data));
        }
    }
}
