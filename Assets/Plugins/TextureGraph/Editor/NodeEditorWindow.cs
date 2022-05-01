using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ViJ.GraphEditor
{
    public class NodeEditorWindow : EditorWindow
    {
        [MenuItem("Windows/NodeEditor")]
        public static void ShowExample()
        {
            NodeEditorWindow wnd = GetWindow<NodeEditorWindow>();
            wnd.titleContent = new GUIContent("NodeEditor");
            wnd.ShowPopup();
        }

        GraphElement m_graph;
        GraphNodeElement m_node;

        public void CreateGUI()
        {
            var root = rootVisualElement;

            var graph = new GraphElement();
            m_graph = graph;
            root.Add(graph);
            graph.StretchToParentSize();

            var node = new GraphNodeElement();
            m_node = node;
            graph.AddToBlackboard(node);
            node.transform.position = new Vector3(100, 100, 0);

            var manipulator = new GraphManipulator(graph);
        }
    }
}

