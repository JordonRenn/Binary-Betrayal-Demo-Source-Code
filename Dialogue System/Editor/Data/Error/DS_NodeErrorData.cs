using System.Collections.Generic;
public class DS_NodeErrorData
{
    public DS_ErrorData errorData { get; set; }

    public List<DS_Node> Nodes { get; set; }

    public DS_NodeErrorData()
    {
        errorData = new DS_ErrorData();
        Nodes = new List<DS_Node>();
    }
}
