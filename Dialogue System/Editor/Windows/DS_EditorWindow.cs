using UnityEditor;
using UnityEngine.UIElements;

#region DS_EditorWindow
public class DS_EditorWindow : EditorWindow
{
    [MenuItem("Window/DS/Dialogue Graph")]
    public static void ShowExample()
    {
        GetWindow<DS_EditorWindow>("Dialogue Graph");
    }

    void OnEnable()
    {
        AddGraphView();
        AddStyles();
    }

    #region Add Elements
    private void AddGraphView()
    {
        DS_GraphView graphView = new DS_GraphView(this);
        graphView.StretchToParentSize();
        rootVisualElement.Add(graphView);
    }

    private void AddStyles()
    {
        rootVisualElement.AddStyleSheets("DialogueSystem/DS_Variables.uss");
    }
    #endregion
}
#endregion