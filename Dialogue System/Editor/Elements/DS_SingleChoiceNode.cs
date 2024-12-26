using UnityEngine;
using UnityEditor.Experimental.GraphView;

public class DS_SingleChoiceNode : DS_Node
{
    public override void Initialize(DS_GraphView _graphView, Vector2 position)
    {
        base.Initialize(_graphView, position);

        DialogueType = DS_DialogueType.SingleChoice;

        Choices.Add("Next Dialogue");
    }
    
    public override void Draw()
    {
        base.Draw();

        // OUTPUT CONTAINER
        foreach (string choice in Choices)
        {
            Port choicePort = this.CreatePort(choice, Orientation.Horizontal, Direction.Output, Port.Capacity.Single);
            outputContainer.Add(choicePort);
        }
        RefreshExpandedState();
    }
}
