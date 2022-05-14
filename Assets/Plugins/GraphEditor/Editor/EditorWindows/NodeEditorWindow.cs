using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ViJ.GraphEditor
{
    public class GraphEditorWindow : EditorWindow
    {
        [MenuItem("Windows/GraphEditor")]
        public static void CreateGraphWindow()
        {
            GraphEditorWindow wnd = GetWindow<GraphEditorWindow>();
            wnd.titleContent = new GUIContent("GraphEditor");
            wnd.ShowPopup();
        }

        private GraphElement m_Graph;

        public void CreateGUI()
        {
            Debug.Log(GraphEditorSettings.Instance.PluginPath);
            var root = rootVisualElement;

            m_Graph = new GraphElement();
            root.Add(m_Graph);
            m_Graph.StretchToParentSize();

            var node = new NodeElement();
            m_Graph.AddNode(node);
            node.transform.position = new Vector3(100, 100, 0);
        }
    }
}