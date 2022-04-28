using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ViJ.GraphEditor
{
    public class NodeEditorWindow : EditorWindow
    {
        [SerializeField] private VisualTreeAsset m_BackgroundTreeAsset = default;

        [MenuItem("Windows/NodeEditor")]
        public static void ShowExample()
        {
            NodeEditorWindow wnd = GetWindow<NodeEditorWindow>();
            wnd.titleContent = new GUIContent("NodeEditor");
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

            var bg = m_BackgroundTreeAsset.Instantiate();
            bg.StretchToParentSize();
            graph.AddToBg(bg);

            var node = new GraphNodeElement();
            m_node = node;
            graph.AddToBlackboard(node);
            node.transform.position = new Vector3(100, 100, 0);
        }

        public void OnGUI()
        {
            m_graph.Scale *= 0.999f;
            m_graph.Position += Vector2.right * 0.1f;
        }
    }
}

