using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DS_NodeData
{
    public Guid _NodeId;
    public DS_DialogueType _DialogueType;
    public string _DialogueName;
    public List<string> _Choices;
    public string _DialogueText;
    public Vector2 _Position;
    public List<Guid> _ConnectedNodeIds; // Store IDs of connected nodes
}
