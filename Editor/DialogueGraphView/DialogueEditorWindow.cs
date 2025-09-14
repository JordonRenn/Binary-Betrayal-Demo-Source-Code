using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;
using System.IO;
using System.Linq;
using System.Collections.Generic;

public class DialogueEditorWindow : EditorWindow
{
    private DialogueGraphView graphView;
    private string fileName = "New Dialogue";
    private StyleSheet variablesStyleSheet;

    [MenuItem("Window/Dialogue Editor")]
    public static void OpenWindow()
    {
        var window = GetWindow<DialogueEditorWindow>("Dialogue Editor");
        window.minSize = new Vector2(800, 600);
    }

    private void CreateGUI()
    {
        // Load styles
        variablesStyleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Editor/DialogueGraphView/DialogueStyleVariables.uss");
        
        GenerateToolbar();
        AddGraphView();
    }

    private void AddGraphView()
    {
        graphView = new DialogueGraphView
        {
            name = "Dialogue Graph"
        };
        graphView.styleSheets.Add(variablesStyleSheet);
        graphView.style.flexGrow = 1;
        rootVisualElement.Add(graphView);
    }

    private void GenerateToolbar()
    {
        var toolbar = new Toolbar();
        toolbar.AddToClassList("dialogue-toolbar");
        
        // File Menu Dropdown
        var fileMenuDropdown = new ToolbarMenu { text = "File" };
        
        fileMenuDropdown.menu.AppendAction("New", action => NewGraph());
        fileMenuDropdown.menu.AppendAction("Save", action => SaveGraph());
        fileMenuDropdown.menu.AppendAction("Save As", action => SaveGraphAs());
        fileMenuDropdown.menu.AppendAction("Load", action => LoadGraph());
        
        toolbar.Add(fileMenuDropdown);

        // Add filename display
        var fileNameTextField = new TextField("File Name:")
        {
            value = fileName,
            style = { marginLeft = 10, marginRight = 10 }
        };
        fileNameTextField.RegisterValueChangedCallback(evt => fileName = evt.newValue);
        toolbar.Add(fileNameTextField);

        rootVisualElement.Add(toolbar);
    }

    private void NewGraph()
    {
        if (EditorUtility.DisplayDialog("New Dialogue", 
            "Are you sure you want to create a new dialogue? Any unsaved changes will be lost.", 
            "Yes", "Cancel"))
        {
            fileName = "New Dialogue";
            graphView.DeleteElements(graphView.nodes.ToList());
            graphView.DeleteElements(graphView.edges.ToList());
        }
    }

    private void SaveGraph()
    {
        if (string.IsNullOrEmpty(fileName) || fileName == "New Dialogue")
        {
            SaveGraphAs();
            return;
        }

        string path = Path.Combine(Application.streamingAssetsPath, "Dialogues", $"{fileName}.json");
        SaveToFile(path);
    }

    private void SaveGraphAs()
    {
        string path = EditorUtility.SaveFilePanel(
            "Save Dialogue Graph",
            Path.Combine(Application.streamingAssetsPath, "Dialogues"),
            fileName,
            "json"
        );

        if (!string.IsNullOrEmpty(path))
        {
            fileName = Path.GetFileNameWithoutExtension(path);
            SaveToFile(path);
        }
    }

    private void SaveToFile(string path)
    {
        // Ensure directory exists
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        
        var graphData = new DialogueGraphData();
        
        // Save nodes
        foreach (var node in graphView.nodes.ToList())
        {
            if (node is DialogueBaseNode dialogueNode)
            {
                var nodeData = new DialogueNodeData
                {
                    id = dialogueNode.GUID,
                    type = dialogueNode.Type,
                    characterName = dialogueNode.characterName,
                    message = dialogueNode.message,
                    position = dialogueNode.GetPosition().position
                };

                // Handle specific node types
                if (dialogueNode is DialogueChoiceNode choiceNode)
                {
                    nodeData.choices = new List<ChoiceData>();
                    for (int choiceIndex = 0; choiceIndex < choiceNode.choices.Count; choiceIndex++)
                    {
                        var choice = choiceNode.choices[choiceIndex];
                        var choiceData = new ChoiceData
                        {
                            choiceType = choice.choiceType,
                            text = choice.text,
                            nextDialogueId = choice.nextDialogueId
                        };

                        // Find the connected node for this choice
                        string choicePortName = $"Choice {choiceIndex + 1} Output";
                        var connectedEdge = graphView.edges.ToList().FirstOrDefault(edge => 
                            edge.output.node == dialogueNode && 
                            edge.output.portName == choicePortName);
                        
                        if (connectedEdge != null && connectedEdge.input.node is DialogueBaseNode connectedNode)
                        {
                            choiceData.outputNodeId = connectedNode.GUID;
                            Debug.Log($"Choice {choiceIndex + 1} connected to node: {connectedNode.GUID}");
                        }
                        else
                        {
                            Debug.LogWarning($"No connection found for choice {choiceIndex + 1} with port name: {choicePortName}");
                            // Debug all edges for this node
                            var nodeEdges = graphView.edges.ToList().Where(edge => edge.output.node == dialogueNode).ToList();
                            Debug.Log($"Found {nodeEdges.Count} edges from this node:");
                            foreach (var edge in nodeEdges)
                            {
                                Debug.Log($"  Port: '{edge.output.portName}' -> {((DialogueBaseNode)edge.input.node).GUID}");
                            }
                        }

                        // Add specific choice type data
                        if (choice is ChoiceGiveItem giveItem)
                        {
                            choiceData.itemId = giveItem.itemId;
                            choiceData.quantity = giveItem.quantity;
                        }
                        else if (choice is ChoiceTakeItem takeItem)
                        {
                            choiceData.itemId = takeItem.itemId;
                            choiceData.quantity = takeItem.quantity;
                        }
                        else if (choice is ChoiceStartQuest questChoice)
                        {
                            choiceData.questId = questChoice.questId;
                        }

                        nodeData.choices.Add(choiceData);
                    }
                }
                else if (dialogueNode is DialogueLoadNewDialogueNode loadNode)
                {
                    nodeData.nextDialogueId = loadNode.nextDialogueId;
                }

                graphData.nodes.Add(nodeData);
            }
        }

        // Save connections
        foreach (var edge in graphView.edges.ToList())
        {
            if (edge.output.node is DialogueBaseNode outputNode && 
                edge.input.node is DialogueBaseNode inputNode)
            {
                var connectionData = new DialogueConnectionData
                {
                    outputNodeId = outputNode.GUID,
                    inputNodeId = inputNode.GUID,
                    outputPortName = edge.output.portName,
                    inputPortName = edge.input.portName
                };
                graphData.connections.Add(connectionData);
            }
        }

        // Convert to JSON and save
        string jsonContent = JsonUtility.ToJson(graphData, true);
        File.WriteAllText(path, jsonContent);
        Debug.Log($"Saved dialogue to: {path}");
    }

    private void LoadGraph()
    {
        string path = EditorUtility.OpenFilePanel(
            "Load Dialogue Graph",
            Path.Combine(Application.streamingAssetsPath, "Dialogues"),
            "json"
        );

        if (!string.IsNullOrEmpty(path))
        {
            if (EditorUtility.DisplayDialog("Load Dialogue", 
                "Loading a new dialogue will replace the current graph. Any unsaved changes will be lost.", 
                "Continue", "Cancel"))
            {
                fileName = Path.GetFileNameWithoutExtension(path);
                LoadFromFile(path);
            }
        }
    }

    private void LoadFromFile(string path)
    {
        if (!File.Exists(path))
        {
            EditorUtility.DisplayDialog("Error", "File not found!", "OK");
            return;
        }

        // Clear current graph
        graphView.DeleteElements(graphView.nodes.ToList());
        graphView.DeleteElements(graphView.edges.ToList());

        // Load and parse JSON
        string jsonContent = File.ReadAllText(path);
        var graphData = JsonUtility.FromJson<DialogueGraphData>(jsonContent);

        // Dictionary to store nodes for connection later
        var nodeLookup = new Dictionary<string, DialogueBaseNode>();

        // Create nodes
        foreach (var nodeData in graphData.nodes)
        {
            DialogueBaseNode node = null;

            // Create the appropriate node type
            switch (nodeData.type)
            {
                case DialogueType.Choice:
                    var choiceNode = new DialogueChoiceNode();
                    if (nodeData.choices != null)
                    {
                        foreach (var choiceData in nodeData.choices)
                        {
                            Choice choice = null;
                            switch (choiceData.choiceType)
                            {
                                case ChoiceType.GiveItem:
                                    choice = new ChoiceGiveItem
                                    {
                                        itemId = choiceData.itemId,
                                        quantity = choiceData.quantity
                                    };
                                    break;
                                case ChoiceType.TakeItem:
                                    choice = new ChoiceTakeItem
                                    {
                                        itemId = choiceData.itemId,
                                        quantity = choiceData.quantity
                                    };
                                    break;
                                case ChoiceType.StartQuest:
                                    choice = new ChoiceStartQuest
                                    {
                                        questId = choiceData.questId
                                    };
                                    break;
                                default:
                                    choice = new Choice();
                                    break;
                            }
                            choice.text = choiceData.text;
                            choice.nextDialogueId = choiceData.nextDialogueId;
                            choiceNode.choices.Add(choice);
                        }
                    }
                    node = choiceNode;
                    break;

                case DialogueType.LoadNewDialogue:
                    var loadNode = new DialogueLoadNewDialogueNode();
                    loadNode.nextDialogueId = nodeData.nextDialogueId;
                    node = loadNode;
                    break;

                default:
                    node = new DialogueBaseNode();
                    break;
            }

            // Set common properties
            node.GUID = nodeData.id;
            node.Type = nodeData.type;
            node.characterName = nodeData.characterName;
            node.message = nodeData.message;

            // Add node to graph
            node.Initialize(nodeData.position);
            node.Draw();
            graphView.AddElement(node);

            // Store for connections
            nodeLookup[node.GUID] = node;
        }

        // Create connections
        foreach (var connectionData in graphData.connections)
        {
            if (nodeLookup.TryGetValue(connectionData.outputNodeId, out var outputNode) &&
                nodeLookup.TryGetValue(connectionData.inputNodeId, out var inputNode))
            {
                // Find the correct ports
                var outputPort = outputNode.outputContainer.Children()
                    .OfType<Port>()
                    .FirstOrDefault(p => p.portName == connectionData.outputPortName);
                var inputPort = inputNode.inputContainer.Children()
                    .OfType<Port>()
                    .FirstOrDefault(p => p.portName == connectionData.inputPortName);

                if (outputPort != null && inputPort != null)
                {
                    var edge = new Edge
                    {
                        output = outputPort,
                        input = inputPort
                    };
                    edge.input.Connect(edge);
                    edge.output.Connect(edge);
                    graphView.AddElement(edge);
                }
            }
        }

        Debug.Log($"Loaded dialogue from: {path}");
    }
}
