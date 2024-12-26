using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Linq;
using System;

#region DS_GraphView
public class DS_GraphView : GraphView
{
    private DS_EditorWindow editorWindow;
    private DS_SearchWindow searchWindow;

    private Dictionary<Guid, DS_NodeData> allNodes = new Dictionary<Guid, DS_NodeData>();
    private Dictionary<Guid, DS_GroupData> allGroups = new Dictionary<Guid, DS_GroupData>();
    
    public DS_GraphView(DS_EditorWindow _editorWindow)
    {
        editorWindow = _editorWindow;
        
        AddManipulators();
        AddSearchWindow();
        AddGridBackground();
        OnElementsDeleted();

        AddStyles();
    }

    #region Overrride Methods
    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
    {
        List<Port> compatiblePorts = new List<Port>();

        ports.ForEach((port) =>
        {
            if (startPort == port || startPort.node == port.node || startPort.direction == port.direction)
            {
                return;
            }

            compatiblePorts.Add(port);
        });
        return compatiblePorts;
    }

    public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
    {
        base.BuildContextualMenu(evt);

        if (evt.target is DS_Node node)
        {
            evt.menu.InsertAction(0, "Disconnect All Input Ports", actionEvent => node.DisconnectAllInputPorts());
            evt.menu.InsertAction(1, "Disconnect All Output Ports", actionEvent => node.DisconnectAllOutputPorts());
        }
    }
    #endregion

    #region Manipulators
    private void AddManipulators()
    {
        SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

        var manipulators = new Manipulator[]
        {
            new ContentDragger(),
            new SelectionDragger(),
            new RectangleSelector(),
            new FreehandSelector(),
            new ClickSelector()
        };

        foreach (var manipulator in manipulators)
        {
            this.AddManipulator(manipulator);
        }

        this.AddManipulator(CreateNodeContextMenu("Add Single Choice Node", DS_DialogueType.SingleChoice));
        this.AddManipulator(CreateNodeContextMenu("Add Multiple Choice Node", DS_DialogueType.MultiChoice));
        this.AddManipulator(CreateGroupContextualMenu());
    }
    #endregion

    #region Add Elements
    private void AddGridBackground()
    {
        GridBackground grid = new GridBackground(); 
        grid.StretchToParentSize();
        Insert(0, grid);
    }

    private void AddStyles()
    {
        this.AddStyleSheets(
            "DialogueSystem/DS_GraphViewStyles.uss",
            "DialogueSystem/DS_NodeStyles.uss"
        );
    }

    private void AddSearchWindow()
    {
        if (searchWindow == null)
        {
            searchWindow = ScriptableObject.CreateInstance<DS_SearchWindow>();
            searchWindow.Initialize(this);
        }

        nodeCreationRequest = context => SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), searchWindow);
    }
    #endregion

    #region Element Creation
    public DS_Node CreateNode(DS_DialogueType type, UnityEngine.Vector2 position)
    {
        DS_Node node = DS_NodeFactory.CreateNode(type);
        if (node == null)
        {
            Debug.LogError($"Failed to create node of type '{type}'.");
            return null;
        }

        node.Initialize(this, position);
        node.Draw();

        AddNodeToDictionary(node, type);
        
        return node;
    }

    public DS_Group CreateGroup(string title, UnityEngine.Vector2 position)
    {
        DS_Group group = new DS_Group(title, position);

        foreach (GraphElement element in selection)
        {
            if (!(element is DS_Node))
            {
                continue;
            }

            DS_Node node = (DS_Node) element;

            group.AddElement(node);
        }

        AddGroupToDictionary(group);

        return group;
    }
    #endregion

    #region Callbacks
    public void OnElementsDeleted()
    {
        deleteSelection = (operationName, askUser) =>
        {
            List<DS_Node> nodesToDelete = new List<DS_Node>();
            List<DS_Group> groupsToDelete = new List<DS_Group>();
            List<Edge> edgesToDelete = new List<Edge>();
            
            foreach (GraphElement element in selection)
            {
                if (element is DS_Node node)
                {
                    nodesToDelete.Add(node);
                    continue;
                }
                else if (element is DS_Group group)
                {
                    groupsToDelete.Add(group);
                    continue;
                }
                else if (element is Edge edge)
                {
                    edgesToDelete.Add(edge);
                    continue;
                }
            }

            foreach (DS_Node node in nodesToDelete)
            {
                // Remove connected edges
                foreach (Edge edge in edges.ToList())
                {
                    if (edge.input.node == node || edge.output.node == node)
                    {
                        edgesToDelete.Add(edge);
                    }
                }

                RemoveNodeFromDictionary(node);
            }

            foreach (DS_Group group in groupsToDelete)
            {
                List<DS_Node> groupedNodes = new List<DS_Node>();

                foreach (GraphElement element in group.containedElements)
                {
                    //grab all the nodes in (each) group
                    if (!(element is DS_Node))
                    {
                        continue;
                    }
                    groupedNodes.Add((DS_Node) element);
                }
                group.RemoveElements(groupedNodes);
                RemoveGroupFromDictionary(group);
            }

            EditorApplication.delayCall += () =>
            {
                foreach (DS_Node node in nodesToDelete)
                {
                    RemoveElement(node);
                }

                foreach (DS_Group group in groupsToDelete)
                {
                    RemoveElement(group);
                }

                foreach (Edge edge in edgesToDelete)
                {
                    RemoveElement(edge);
                }
            };
        };
    }
    #endregion

    #region Contextual Menus
    private IManipulator CreateNodeContextMenu(string title, DS_DialogueType type)
    {
        ContextualMenuManipulator contextualMenuManipulator = new ContextualMenuManipulator(
            menuEvent => menuEvent.menu.AppendAction(title, actionEvent => AddElement(CreateNode(type, GetLocalMousePoition(actionEvent.eventInfo.localMousePosition))))
        );

        return contextualMenuManipulator;
    }

    private IManipulator CreateGroupContextualMenu()
    {
        ContextualMenuManipulator contextualMenuManipulator = new ContextualMenuManipulator(
            menuEvent => menuEvent.menu.AppendAction("Add Group", actionEvent => AddElement(CreateGroup("Dialogue Group", GetLocalMousePoition(actionEvent.eventInfo.localMousePosition))))
        );

        return contextualMenuManipulator;
    }
    #endregion

    
    #region Manage Dictionary
    private void AddNodeToDictionary(DS_Node node, DS_DialogueType type)
    {
        DS_NodeData nodeData = new DS_NodeData
        {
            _NodeId = node.NodeId,
            _DialogueType = type,
            _DialogueName = node.DialogueName,
            _Choices = node.Choices,
            _DialogueText = node.DialogueText,
            _Position = node.GetPosition().position,
            _ConnectedNodeIds = new List<Guid>() // Populate with connected node IDs
        };
        allNodes[node.NodeId] = nodeData;
        
    }

    private void RemoveNodeFromDictionary(DS_Node node)
    {
        allNodes.Remove(node.NodeId);
    }

    private void AddGroupToDictionary(DS_Group group)
    {
        DS_GroupData groupData = new DS_GroupData
        {
            _GroupId = group.GroupId,
            _Title = group.title,
            _Position = group.GetPosition().position,
            _NodeIds = group.containedElements.OfType<DS_Node>().Select(node => node.NodeId).ToList()
        };
        allGroups[group.GroupId] = groupData;
    }

    private void RemoveGroupFromDictionary(DS_Group group)
    {
        allGroups.Remove(group.GroupId);
    }
    #endregion

    #region Utility
    public Vector2 GetLocalMousePoition(Vector2 mousePosition, bool isSearchWindow = false)
    {
        Vector2 worldMousePosition = mousePosition;

        if (isSearchWindow)
        {
            worldMousePosition -= editorWindow.position.position;
        }

        Vector2 localMousePosition = contentViewContainer.WorldToLocal(worldMousePosition);
        return localMousePosition;
    }
    #endregion
}
#endregion