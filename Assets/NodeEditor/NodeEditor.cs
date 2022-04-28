using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class NodeEditor : EditorWindow
{
    [SerializeField] private VisualTreeAsset m_BackgroundTreeAsset = default;
    [SerializeField] private VisualTreeAsset m_Tree = default;

    [MenuItem("Windows/NodeEditor")]
    public static void ShowExample()
    {
        NodeEditor wnd = GetWindow<NodeEditor>();
        wnd.titleContent = new GUIContent("NodeEditor");
    }

    public void CreateGUI()
    {
        var root = rootVisualElement;
        var background = m_BackgroundTreeAsset.Instantiate();
        root.Add(background);
        background.StretchToParentSize();

        background.style.left = 0;
        background.style.right = 0;
       // background.style.top = 0;
       // background.style.bottom = 0;
      //  background.style.position = Position.Absolute;

        var node = m_Tree.Instantiate();
        background.Add(node);
        node.transform.position = new Vector3(100, 100, 0);
    }
}
