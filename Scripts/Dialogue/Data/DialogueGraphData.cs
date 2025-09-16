using System;
using System.Collections.Generic;
using UnityEngine;

namespace DialogueSystem.Data
{
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
        public string outputNodeId = "";
        public DialogueType type;
        public string characterName;
        public string message;
        public Vector2 position;
        public List<ChoiceData> choices;
        // public string nextDialogueId;
    }

    [Serializable]
    public class DialogueConnectionData
    {
        public string outputNodeId;
        public string inputNodeId;
        public string outputPortName;
        public string inputPortName;
    }

    [Serializable]
    public class ChoiceData : Choice
    {
        //public string outputNodeId;
        public string itemId;
        public int quantity;
        public string questId;
    }
}