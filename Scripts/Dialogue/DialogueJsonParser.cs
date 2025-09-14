using UnityEngine;
using System.Collections.Generic;
using System;
using DialogueSystem.Data;
using DialogueSystem.Graph;

// Temporary structures for Unity JsonUtility fallback
[Serializable]
public class TempDialogueData
{
    public string dialogueId;
    public bool freezePlayer;
    public TempNodeData[] nodes;
}

[Serializable]
public class TempNodeData
{
    public string id;
    public int type;
    public string characterName;
    public string message;
    public TempChoiceData[] choices;
}

[Serializable]
public class TempChoiceData
{
    public int choiceType;
    public string text;
    public string nextDialogueId;
    public string outputNodeId;
    public string itemId;
    public int quantity;
    public string questId;
}

public static unsafe class DialogueJsonParser
{
    public static DialogueGraphData ParseDialogueGraph(string jsonContent)
    {
        if (string.IsNullOrEmpty(jsonContent) || SimdJsonInterop.validate_json(jsonContent) != 1)
        {
            return null;
        }

        var graphData = new DialogueGraphData();

        // Parse basic properties
        graphData.dialogueId = SimdJsonInterop.GetString(jsonContent, "dialogueId");
        int freezePlayerValue = SimdJsonInterop.get_bool(jsonContent, "freezePlayer");
        graphData.freezePlayer = freezePlayerValue == 1;

        // Parse nodes
        int nodesCount = SimdJsonInterop.get_array_length(jsonContent, "nodes");
        for (int i = 0; i < nodesCount; i++)
        {
            var nodeData = new DialogueNodeData
            {
                id = SimdJsonInterop.GetArrayString(jsonContent, "nodes", i, "id"),
                characterName = SimdJsonInterop.GetArrayString(jsonContent, "nodes", i, "characterName"),
                message = SimdJsonInterop.GetArrayString(jsonContent, "nodes", i, "message"),
                nextDialogueId = SimdJsonInterop.GetArrayString(jsonContent, "nodes", i, "nextDialogueId")
            };

            // Parse type as numeric value
            int typeInt = SimdJsonInterop.get_array_int(jsonContent, "nodes", i, "type");
            nodeData.type = typeInt switch
            {
                0 => DialogueType.Text,
                1 => DialogueType.Choice,
                2 => DialogueType.LoadNewDialogue,
                _ => DialogueType.Text // Default fallback
            };

            // Parse position
            float x = SimdJsonInterop.get_int(jsonContent, $"nodes[{i}].position.x");
            float y = SimdJsonInterop.get_int(jsonContent, $"nodes[{i}].position.y");
            nodeData.position = new Vector2(x, y);

            // Parse choices if present
            if (nodeData.type == DialogueType.Choice)
            {
                nodeData.choices = new List<ChoiceData>();
                
                // FALLBACK: Try Unity's JsonUtility for choices parsing
                try
                {
                    // Parse the entire JSON to get to our specific node's choices
                    var tempData = JsonUtility.FromJson<TempDialogueData>(jsonContent);
                    if (tempData != null && tempData.nodes != null && i < tempData.nodes.Length)
                    {
                        var tempNode = tempData.nodes[i];
                        if (tempNode.choices != null && tempNode.choices.Length > 0)
                        {
                            foreach (var tempChoice in tempNode.choices)
                            {
                                var choiceData = new ChoiceData
                                {
                                    text = tempChoice.text,
                                    nextDialogueId = tempChoice.nextDialogueId,
                                    itemId = tempChoice.itemId,
                                    questId = tempChoice.questId,
                                    outputNodeId = tempChoice.outputNodeId,
                                    quantity = tempChoice.quantity
                                };

                                if (Enum.TryParse<ChoiceType>(tempChoice.choiceType.ToString(), true, out var choiceType))
                                {
                                    choiceData.choiceType = choiceType;
                                }

                                nodeData.choices.Add(choiceData);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    SBGDebug.LogError($"Unity fallback failed for node {nodeData.id}: {ex.Message}", "DialogueJsonParser | ParseDialogueGraph");
                }
            }

            graphData.nodes.Add(nodeData);
        }

        // Parse connections
        int connectionsCount = SimdJsonInterop.get_array_length(jsonContent, "connections");
        for (int i = 0; i < connectionsCount; i++)
        {
            var connectionData = new DialogueConnectionData
            {
                outputNodeId = SimdJsonInterop.GetArrayString(jsonContent, "connections", i, "outputNodeId"),
                inputNodeId = SimdJsonInterop.GetArrayString(jsonContent, "connections", i, "inputNodeId"),
                outputPortName = SimdJsonInterop.GetArrayString(jsonContent, "connections", i, "outputPortName"),
                inputPortName = SimdJsonInterop.GetArrayString(jsonContent, "connections", i, "inputPortName")
            };
            graphData.connections.Add(connectionData);
        }

        return graphData;
    }
    public static DialogueData[] ParseDialogueData(TextAsset jsonTextAsset)
    {
        if (jsonTextAsset == null)
        {
            SBGDebug.LogError("Dialogue JSON asset is null!", "DialogueJsonParser");
            return new DialogueData[0];
        }

        string json = jsonTextAsset.text;
        SBGDebug.LogInfo($"Parsing JSON as graph data only", "DialogueJsonParser");

        try
        {
            var graphData = ParseDialogueGraph(json);
            if (graphData != null)
            {
                SBGDebug.LogInfo("Successfully parsed as graph data", "DialogueJsonParser");
                var runtimeData = DialogueGraphConverter.ConvertToRuntimeFormat(graphData);
                return new[] { runtimeData };
            }
            else
            {
                SBGDebug.LogError("Failed to parse dialogue graph - ParseDialogueGraph returned null", "DialogueJsonParser");
                return new DialogueData[0];
            }
        }
        catch (Exception e)
        {
            SBGDebug.LogError($"Failed to parse as graph data: {e.Message}", "DialogueJsonParser");
            SBGDebug.LogException(e, "DialogueJsonParser");
            return new DialogueData[0];
        }
    }
}