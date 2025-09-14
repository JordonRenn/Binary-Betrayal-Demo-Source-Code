using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DialogueGraphData
{
    public string dialogueId;
    public bool freezePlayer = false;
    public List<DialogueNodeData> nodes = new List<DialogueNodeData>();
    public List<DialogueConnectionData> connections = new List<DialogueConnectionData>();
}

[Serializable]
public class DialogueNodeData
{
    public string id;
    public DialogueType type;
    public string characterName;
    public string message;
    public Vector2 position;
    
    // For choice nodes
    public List<ChoiceData> choices;
    
    // For load new dialogue nodes
    public string nextDialogueId;
}

[Serializable]
public class DialogueConnectionData
{
    public string outputNodeId;
    public string inputNodeId;
    public string outputPortName; // For choice nodes, this will be "Choice X Output"
    public string inputPortName;
}

[Serializable]
public class ChoiceData : Choice
{
    public string outputNodeId; // The ID of the node this choice connects to
    public string itemId;
    public int quantity;
    public string questId;
}