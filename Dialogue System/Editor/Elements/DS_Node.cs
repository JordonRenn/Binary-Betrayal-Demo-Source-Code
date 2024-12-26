using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

public enum DS_DialogueType
{
    SingleChoice,
    MultiChoice
}

#region DS_Node
public class DS_Node : Node
{
    public Guid NodeId { get; private set; }
    public string DialogueName { get; set; }
    public List<string> Choices { get; set; }
    public string DialogueText { get; set; }
    public DS_DialogueType DialogueType { get; set; }

    protected DS_GraphView graphView;

    public virtual void Initialize(DS_GraphView _graphView, Vector2 position)
    {
        NodeId = Guid.NewGuid();

        DialogueName = "Dialogue Node";

        Choices = new List<string>();
        DialogueText = "Dialogue text...";

        SetPosition(new Rect(position, UnityEngine.Vector2.zero));

        graphView = _graphView;

        mainContainer.AddToClassList("ds-node__main-container");
        extensionContainer.AddToClassList("ds-node__extension-container");
    }

    public virtual void Draw()
    {
        // Clear the elements
        ClearElements();

        // TITLE CONTAINER
        TextField dialogueNameTextField = DS_ElementUtility.CreateTextField(DialogueName);

        dialogueNameTextField.AddClasses(
            "ds-node__textfield",
            "ds-node__filename-textfield",
            "ds-node__textfield__hidden"
        );
        titleContainer.Insert(0, dialogueNameTextField);

        // INPUT CONTAINER
        Port inputPort = this.CreatePort("Dialogue Connection", Orientation.Horizontal, Direction.Input, Port.Capacity.Multi);

        inputPort.portName = "Dialogue Connection";
        inputContainer.Add(inputPort);

        //EXTENSION CONTAINER
        VisualElement customDataContainer = new VisualElement();

        customDataContainer.AddToClassList("ds-node__custom-data-container");

        Foldout textFoldout = DS_ElementUtility.CreateFoldout("Dialogue Text");

        TextField textTextField = DS_ElementUtility.CreateTextArea(DialogueText);

        textTextField.AddClasses(
            "ds-node__textfield",
            "ds-node__quote-textfield"
        );

        textFoldout.Add(textTextField);
        customDataContainer.Add(textFoldout);
        extensionContainer.Add(customDataContainer);
    }

    public void DisconnectAllPorts()
    {
        DisconnectPorts(inputContainer);
        DisconnectPorts(outputContainer);
    }

    public void DisconnectAllInputPorts()
    {
        DisconnectPorts(inputContainer);
    }

    public void DisconnectAllOutputPorts()
    {
        DisconnectPorts(outputContainer);
    }

    private void DisconnectPorts(VisualElement container)
    {
        foreach (Port port in container.Children())
        {
            if (!port.connected)
            {
                continue;
            }

            graphView.DeleteElements(port.connections);
        }
    }

    #region ERROR HANDLING
    private void ClearElements()
    {
        // Clear the elements
        inputContainer.Clear();
        outputContainer.Clear();
    }

    public void SetErrorStyle(Color color)
    {
        mainContainer.style.backgroundColor = new Color(255f / 255f, 51f / 255f, 51f / 255f);
    }

    public void ResetErrorStyle()
    {
        mainContainer.style.backgroundColor = new Color(40f / 255f, 41f / 255f, 42f / 255f);
    }
    #endregion
}
#endregion