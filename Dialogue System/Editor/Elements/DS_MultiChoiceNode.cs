using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

public class DS_MultiChoiceNode : DS_Node
{
    public override void Initialize(DS_GraphView _graphView, Vector2 position)
    {
        base.Initialize(_graphView, position);

        DialogueType = DS_DialogueType.MultiChoice;

        Choices.Add("New Choice");
    }

    public override void Draw()
    {
        base.Draw();

        //MAIN CONTAINER
        Button addChoiceButton = DS_ElementUtility.CreateButton("Add Choice", () => 
        {
            Port choicePort = CreateChoicePort("New Choice");

            Choices.Add("New Choice");

            outputContainer.Add(choicePort);

        });
        addChoiceButton.AddToClassList("ds-node__button");
        mainContainer.Insert(1, addChoiceButton);

        // OUTPUT CONTAINER
        foreach (string choice in Choices)
        {
            Port choicePort = CreateChoicePort(choice);

            outputContainer.Add(choicePort);
        }
        RefreshExpandedState();
    }

    #region Element Creation
    private Port CreateChoicePort(string choice)
    {
        Port choicePort = this.CreatePort("", Orientation.Horizontal, Direction.Output, Port.Capacity.Single);
        choicePort.portName = "";

        //delete button
        Button deleteChoiceButton = DS_ElementUtility.CreateButton("X");
        deleteChoiceButton.AddToClassList("ds-node__button");

        //text field
        TextField choiceTextField = DS_ElementUtility.CreateTextField(choice);
        
        choiceTextField.AddClasses(
            "ds-node__textfield",
            "ds-node__choice-textfield",
            "ds-node__textfield__hidden"
        );

        //assign to output container
        choicePort.Add(choiceTextField);
        choicePort.Add(deleteChoiceButton);
        outputContainer.Add(choicePort);

        return choicePort;
    }
    #endregion
}
