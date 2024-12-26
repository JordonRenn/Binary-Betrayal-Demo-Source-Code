using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DS_GroupData
{
    public Guid _GroupId;
    public string _Title;
    public Vector2 _Position;
    public List<Guid> _NodeIds; // Store IDs of nodes in the group
}
