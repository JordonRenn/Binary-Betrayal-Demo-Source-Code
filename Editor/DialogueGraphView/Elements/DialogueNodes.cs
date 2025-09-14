using UnityEditor.Experimental.GraphView;
using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEngine;
using UnityEditor;
using System.Linq;

public class DialogueBaseNode : Node
{
    public DialogueType Type { get; set; } = DialogueType.Text;
    public string characterName { get; set; }
    public string message { get; set; }
    public string GUID { get; internal set; }

    private StyleSheet nodeStyleSheet;

    #region Base Node
    public DialogueBaseNode()
    {
        GUID = System.Guid.NewGuid().ToString();
        nodeStyleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Editor/DialogueGraphView/DialogueNodeStyles.uss");
        styleSheets.Add(nodeStyleSheet);
        AddToClassList("dialogue-node");
    }

    public void Initialize(Vector2 position)
    {
        SetPosition(new Rect(position, Vector2.zero));
        name = "Dialogue Node";
        characterName = string.Empty;
        message = string.Empty;
    }

    public void Draw()
    {
        /* TITLE  */

        TextField titleName = new TextField(name);
        titleName.AddToClassList("dialogue-node__title-text");
        titleContainer.AddToClassList("dialogue-node__title");
        titleContainer.Insert(0, titleName);

        /* PORTS */

        Port inputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(DialogueBaseNode));
        inputPort.portName = "Input";
        inputPort.AddToClassList("dialogue-node__input");
        inputPort.portColor = Color.green; // Makes it clearer this is a multi-input port
        inputContainer.Add(inputPort);

        Port outputPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(DialogueBaseNode));
        outputPort.portName = "Output";
        outputPort.AddToClassList("dialogue-node__output");
        outputContainer.Add(outputPort);

        /* DATA CONTAINER */

        VisualElement dataContainer = new VisualElement();
        dataContainer.AddToClassList("dialogue-node__data-container");

        /* CHARACTER NAME */

        TextField characterNameField = new TextField("Character Name:");
        characterNameField.AddToClassList("dialogue-node__character-field");
        characterNameField.value = characterName;
        characterNameField.RegisterValueChangedCallback(evt => characterName = evt.newValue);

        dataContainer.Add(characterNameField);

        /* MESSAGE */

        TextField messageField = new TextField("Message:");
        messageField.AddToClassList("dialogue-node__message-field");
        messageField.multiline = true;
        messageField.value = message;
        messageField.RegisterValueChangedCallback(evt => message = evt.newValue);

        Foldout messageFoldout = new Foldout { text = "Character Details", value = true };
        messageFoldout.Add(messageField);
        dataContainer.Add(messageFoldout);

        extensionContainer.Add(dataContainer);
        DrawAdditionalFields();
        RefreshExpandedState();
    }

    protected virtual void DrawAdditionalFields()
    {
        // do thing for base class
    }
}
#endregion

#region Choice Node
public class DialogueChoiceNode : DialogueBaseNode
{
    public List<Choice> choices { get; set; } = new List<Choice>();
    public DialogueChoiceNode()
    {
        Type = DialogueType.Choice;
        name = "Choice Node";
        characterName = string.Empty;
        message = string.Empty;
        choices = new List<Choice>();
        AddToClassList("dialogue-node--choice");
    }

    public new void Initialize(Vector2 position)
    {
        SetPosition(new Rect(position, Vector2.zero));
        name = "Choice Node";
        characterName = string.Empty;
        message = string.Empty;
        choices = new List<Choice>();
    }

    protected override void DrawAdditionalFields()
    {
        /* CHOICES */

        VisualElement choicesContainer = new VisualElement();
        choicesContainer.AddToClassList("dialogue-node__choice-container");
        choicesContainer.style.flexDirection = FlexDirection.Column;

        VisualElement addChoiceContainer = new VisualElement();
        addChoiceContainer.style.flexDirection = FlexDirection.Row;
        
        var choiceTypeEnum = System.Enum.GetValues(typeof(ChoiceType));
        DropdownField choiceTypeDropdown = new DropdownField("Choice Type");
        choiceTypeDropdown.choices = new List<string>();
        foreach (ChoiceType type in choiceTypeEnum)
        {
            choiceTypeDropdown.choices.Add(type.ToString());
        }
        choiceTypeDropdown.value = choiceTypeDropdown.choices[0];
        
        Button addChoiceButton = new Button(() =>
        {
            Choice newChoice;
            switch (System.Enum.Parse<ChoiceType>(choiceTypeDropdown.value))
            {
                case ChoiceType.GiveItem:
                    newChoice = new ChoiceGiveItem();
                    break;
                case ChoiceType.TakeItem:
                    newChoice = new ChoiceTakeItem();
                    break;
                case ChoiceType.StartQuest:
                    newChoice = new ChoiceStartQuest();
                    break;
                default:
                    newChoice = new Choice();
                    break;
            }
            choices.Add(newChoice);
            RefreshChoices(choicesContainer);
        })
        { text = "Add Choice" };

        addChoiceContainer.Add(choiceTypeDropdown);
        addChoiceContainer.Add(addChoiceButton);
        choicesContainer.Add(addChoiceContainer);
        RefreshChoices(choicesContainer);

        Foldout choicesFoldout = new Foldout { text = "Choices", value = true };
        choicesFoldout.Add(choicesContainer);
        extensionContainer.Add(choicesFoldout);
    }

    private void RefreshChoices(VisualElement container)
    {
        // Clear existing choice fields except the add choice container
        while (container.childCount > 1)
        {
            container.RemoveAt(1);
        }

        for (int i = 0; i < choices.Count; i++)
        {
            int index = i; // Capture index for the closure
            Choice choice = choices[i];

            VisualElement choiceElement = new VisualElement();
            choiceElement.style.flexDirection = FlexDirection.Column;
            choiceElement.style.marginTop = 5;
            
            // Add choice text field
            TextField choiceTextField = new TextField($"Choice {i + 1}:");
            choiceTextField.value = choice.text;
            choiceTextField.RegisterValueChangedCallback(evt => choice.text = evt.newValue);
            choiceElement.Add(choiceTextField);

            // Add type-specific fields
            switch (choice)
            {
                case ChoiceGiveItem giveItem:
                    TextField itemIdField = new TextField("Item ID:");
                    itemIdField.value = giveItem.itemId;
                    itemIdField.RegisterValueChangedCallback(evt => giveItem.itemId = evt.newValue);
                    
                    IntegerField quantityField = new IntegerField("Quantity:");
                    quantityField.value = giveItem.quantity;
                    quantityField.RegisterValueChangedCallback(evt => giveItem.quantity = evt.newValue);
                    
                    choiceElement.Add(itemIdField);
                    choiceElement.Add(quantityField);
                    break;

                case ChoiceTakeItem takeItem:
                    TextField takeItemIdField = new TextField("Item ID:");
                    takeItemIdField.value = takeItem.itemId;
                    takeItemIdField.RegisterValueChangedCallback(evt => takeItem.itemId = evt.newValue);
                    
                    IntegerField takeQuantityField = new IntegerField("Quantity:");
                    takeQuantityField.value = takeItem.quantity;
                    takeQuantityField.RegisterValueChangedCallback(evt => takeItem.quantity = evt.newValue);
                    
                    choiceElement.Add(takeItemIdField);
                    choiceElement.Add(takeQuantityField);
                    break;

                case ChoiceStartQuest startQuest:
                    TextField questIdField = new TextField("Quest ID:");
                    questIdField.value = startQuest.questId;
                    questIdField.RegisterValueChangedCallback(evt => startQuest.questId = evt.newValue);
                    
                    choiceElement.Add(questIdField);
                    break;
            }

            // Add output port for next dialogue
            Port choicePort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(DialogueBaseNode));
            choicePort.portName = $"Choice {i + 1} Output";
            choiceElement.Add(choicePort);

            // Add remove button
            Button removeChoiceButton = new Button(() =>
            {
                // Get the parent GraphView
                var graphView = GetFirstAncestorOfType<DialogueGraphView>();
                if (graphView != null)
                {
                    // Find and remove all connections from this choice's output port
                    var port = (Port)choiceElement.Children().FirstOrDefault(x => x is Port);
                    if (port != null)
                    {
                        // Remove all edges connected to this port
                        foreach (Edge edge in port.connections.ToList())
                        {
                            graphView.RemoveElement(edge);
                        }
                    }
                }

                choices.RemoveAt(index);
                RefreshChoices(container);
            })
            { text = "Remove Choice" };
            choiceElement.Add(removeChoiceButton);

            container.Add(choiceElement);
        }
    }
}
#endregion

#region Load New Node
public class DialogueLoadNewDialogueNode : DialogueBaseNode
{
    public string nextDialogueId { get; set; }
    public DialogueLoadNewDialogueNode()
    {
        Type = DialogueType.LoadNewDialogue;
        name = "Load New Dialogue Node";
        characterName = string.Empty;
        message = string.Empty;
        nextDialogueId = string.Empty;
        AddToClassList("dialogue-node--load-new");
    }

    public new void Initialize(Vector2 position)
    {
        SetPosition(new Rect(position, Vector2.zero));
        name = "Load New Dialogue Node";
        characterName = string.Empty;
        message = string.Empty;
        nextDialogueId = string.Empty;
    }

    protected override void DrawAdditionalFields()
    {
        /* NEXT DIALOGUE ID */

        TextField nextDialogueIdField = new TextField("Next Dialogue ID:");
        nextDialogueIdField.AddToClassList("dialogue-node__next-dialogue-field");
        nextDialogueIdField.value = nextDialogueId;
        nextDialogueIdField.RegisterValueChangedCallback(evt => nextDialogueId = evt.newValue);

        extensionContainer.Add(nextDialogueIdField);
    }
}
#endregion