using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ViJ.GraphEditor
{
    public class GraphEditorWindow : EditorWindow
    {
        [MenuItem("Windows/GraphEditor")]
        public static void ShowExample()
        {
            GraphEditorWindow wnd = GetWindow<GraphEditorWindow>();
            wnd.titleContent = new GUIContent("GraphEditor");
            wnd.ShowPopup();
        }

        GraphElement m_graph;

        public void CreateGUI()
        {
            Debug.Log(GraphEditorSettings.Instance.PluginPath);
            var root = rootVisualElement;

            var graph = new GraphElement();
            m_graph = graph;
            root.Add(graph);
            graph.StretchToParentSize();

            var node = new NodeElement();
            graph.AddNode(node);
            node.AddManipulator(new NodeManipulator(node, graph));
            node.transform.position = new Vector3(100, 100, 0);

            var manipulator = new GraphManipulator(graph);
            graph.AddManipulator(manipulator);
        }
    }
}

