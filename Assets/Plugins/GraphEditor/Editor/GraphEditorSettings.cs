using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ViJ.GraphEditor
{
    [CreateAssetMenu(fileName = nameof(GraphEditorSettings), menuName = "Create " + nameof(GraphEditorSettings), order = 0)]
    public class GraphEditorSettings : ScriptableObject
    {
        private static GraphEditorSettings m_Instance;

        public static GraphEditorSettings Instance
        {
            get
            {
                if (m_Instance == null)
                {
                    var settingsGUID = AssetDatabase.FindAssets($"{nameof(GraphEditorSettings)} t:ScriptableObject").First();
                    var settingsPath = AssetDatabase.GUIDToAssetPath(settingsGUID);
                    m_Instance = AssetDatabase.LoadAssetAtPath<GraphEditorSettings>(settingsPath);
                    m_Instance.PluginPath = Path.GetDirectoryName(settingsPath); ;
                }
                return m_Instance;
            }
        }

        public string PluginPath { get; private set; }

        private GraphEditorSettings()
        { }
    }
}