using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

#region DS_SearchWindow
public class DS_SearchWindow : ScriptableObject, ISearchWindowProvider
{
    private DS_GraphView graphView;
    private Texture2D indentationIcon;

    public void Initialize (DS_GraphView _graphView)
        {
            graphView = _graphView;

            indentationIcon = new Texture2D(1, 1);
            indentationIcon.SetPixel(0, 0, new Color(0, 0, 0, 0));
            indentationIcon.Apply();
        }
    
    public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
    {
        List<SearchTreeEntry> searchTreeEntries = new List<SearchTreeEntry>()
        {
            new SearchTreeGroupEntry(new GUIContent("Create Element")),
            new SearchTreeGroupEntry(new GUIContent("Dialogue Node"), 1),
            new SearchTreeEntry(new GUIContent("Single Choice Node", indentationIcon))
            {
                level = 2,
                userData = DS_DialogueType.SingleChoice
            },
            new SearchTreeEntry(new GUIContent("Multi Choice Node", indentationIcon))
            {
                level = 2,
                userData = DS_DialogueType.MultiChoice
            },
            new SearchTreeGroupEntry(new GUIContent("Dialogue Group", indentationIcon), 1),
            new SearchTreeEntry(new GUIContent("Dialogue Group"))
            {
                level = 2,
                userData = new Group()
            }
        };

        return searchTreeEntries;
    }

    public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
    {
        Vector2 mousePosition = graphView.GetLocalMousePoition(context.screenMousePosition, true);
        
        switch (SearchTreeEntry.userData)
        {
            case DS_DialogueType.SingleChoice:
                DS_SingleChoiceNode singleChoiceNode = (DS_SingleChoiceNode) graphView.CreateNode(DS_DialogueType.SingleChoice, mousePosition);
                graphView.AddElement(singleChoiceNode);
                return true;
            case DS_DialogueType.MultiChoice:
                DS_MultiChoiceNode multiChoiceNode = (DS_MultiChoiceNode) graphView.CreateNode(DS_DialogueType.MultiChoice, mousePosition);
                graphView.AddElement(multiChoiceNode);
                return true;
            case DS_Group _:
                DS_Group group = graphView.CreateGroup("Dialogue Group", mousePosition);
                graphView.AddElement(group);
                return true;
            default:
                return false;
        }
    }
}
#endregion