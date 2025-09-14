using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class DialogueGraphView : GraphView
{
    private StyleSheet backgroundStyleSheet;
#if UNITY_EDITOR
    public DialogueGraphView()
    {
        backgroundStyleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Editor/DialogueGraphView/DialogueEditor.uss");
        AddGridBackground();
        AddManipulators();
        
        // Add connection capabilities
        this.AddManipulator(new EdgeManipulator());
        
        // Set up connection validation
        graphViewChanged += OnGraphViewChanged;
        
        // Enable connections between ports
        SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
    }
#endif

    private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange)
    {
        if (graphViewChange.edgesToCreate != null)
        {
            foreach (Edge edge in graphViewChange.edgesToCreate)
            {
                Port inputPort = edge.input;
                Port outputPort = edge.output;

                // Allow multiple connections for input ports
                if (inputPort.capacity == Port.Capacity.Multi)
                {
                    edge.AddToClassList("dialogue-connection");
                }
                // For single capacity ports, remove any existing connections
                else if (inputPort.connections.Count() > 0)
                {
                    // Remove all existing edges
                    foreach (Edge existingEdge in inputPort.connections.ToList())
                    {
                        existingEdge.input.Disconnect(existingEdge);
                        existingEdge.output.Disconnect(existingEdge);
                        RemoveElement(existingEdge);
                    }
                    edge.AddToClassList("dialogue-connection");
                }
            }
        }

        // Handle edge removals
        if (graphViewChange.elementsToRemove != null)
        {
            graphViewChange.elementsToRemove.ForEach(elem =>
            {
                if (elem is Edge edge)
                {
                    edge.input.Disconnect(edge);
                    edge.output.Disconnect(edge);
                }
            });
        }
        return graphViewChange;
    }

    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
    {
        var compatiblePorts = new List<Port>();
        
        ports.ForEach((port) =>
        {
            // Don't connect to self
            if (startPort.node == port.node)
                return;
                
            // Don't connect input to input or output to output
            if (startPort.direction == port.direction)
                return;
                
            compatiblePorts.Add(port);
        });
        
        return compatiblePorts;
    }

    private void AddGridBackground()
    {
        var grid = new GridBackground();
        Insert(0, grid);
        grid.style.flexGrow = 1;
        grid.styleSheets.Add(backgroundStyleSheet);
        grid.AddToClassList("grid-background");
    }

    private void AddManipulators()
    {
        SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());
        this.AddManipulator(new FreehandSelector());

        this.AddManipulator(CreateNodeContextMenu());
    }

    private IManipulator CreateNodeContextMenu()
    {
        var contextMenu = new ContextualMenuManipulator(evt =>
        {
            Vector2 mousePosition = evt.localMousePosition;
            evt.menu.AppendAction("Create Node", action => AddElement(CreateNode(mousePosition)));
            evt.menu.AppendAction("Create Choice Node", action => AddElement(CreateChoiceNode(mousePosition)));
            evt.menu.AppendAction("Create Load New Dialogue Node", action => AddElement(CreateLoadNewNode(mousePosition)));
        });
        return contextMenu;
    }

    public DialogueBaseNode CreateNode(Vector2 position = default)
    {
        var node = new DialogueBaseNode();
        node.Initialize(position);
        node.Draw();
        return node;
    }

    public DialogueChoiceNode CreateChoiceNode(Vector2 position = default)
    {
        var choiceNode = new DialogueChoiceNode();
        choiceNode.Initialize(position);
        choiceNode.Draw();
        return choiceNode;
    }

    public DialogueBaseNode CreateLoadNewNode(Vector2 position = default)
    {
        var loadNewNode = new DialogueLoadNewDialogueNode();
        loadNewNode.Initialize(position);
        loadNewNode.Draw();
        return loadNewNode;
    }
}
