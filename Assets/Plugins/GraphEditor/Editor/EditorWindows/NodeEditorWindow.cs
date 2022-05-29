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

            var node1 = new NodeElement(m_Graph);
            node1.BlackboardPosition = new Vector2(100, 100);

            var node2 = new NodeElement(m_Graph);
            node2.BlackboardPosition = new Vector2(200, 200);

            node1.TryGetPin(0, out var pin0);
            node2.TryGetPin(4, out var pin1);
            m_Graph.ConnectPins(pin0, pin1);
        }
    }
}