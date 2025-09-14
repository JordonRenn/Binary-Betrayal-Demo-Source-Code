using UnityEngine;
using UnityEngine.UIElements;

[UxmlElement]
public partial class DialogueChoiceButton : Button
{
    [UxmlAttribute]
    public string choiceText
    {
        get => _choiceText;
        set
        {
            _choiceText = value;
            this.text = value; // Use Button's built-in text property
        }
    }

    private string _choiceText = "Choice";

    private const string ICON_PATH = "Textures/UI/DialogueChoiceIcon.png"; //in resources folder
    private Image hoverIcon;

    public DialogueChoiceButton()
    {
        // Create icon that only shows on hover
        hoverIcon = new Image();
        hoverIcon.image = Resources.Load<Texture2D>(ICON_PATH);
        hoverIcon.AddToClassList("choice-button-icon");
        hoverIcon.style.display = DisplayStyle.None; // Hidden by default

        this.Add(hoverIcon);
        this.AddToClassList("choice-button");

        // Ensure the button can receive events
        this.focusable = true;
        this.pickingMode = PickingMode.Position;

        // Set up hover behavior for icon
        this.RegisterCallback<MouseEnterEvent>(OnMouseEnter);
        this.RegisterCallback<MouseLeaveEvent>(OnMouseLeave);

        // Add debug logging
        this.RegisterCallback<ClickEvent>(OnDebugClick);

        // Set initial text
        this.text = choiceText;
        
        SBGDebug.LogInfo($"DialogueChoiceButton created with text: {choiceText}", "DialogueChoiceButton | Constructor");
    }

    private void OnDebugClick(ClickEvent evt)
    {
        SBGDebug.LogInfo($"DialogueChoiceButton received ClickEvent: {text}", "DialogueChoiceButton | OnDebugClick");
    }

    private void OnMouseEnter(MouseEnterEvent evt)
    {
        SBGDebug.LogInfo($"Mouse entered button: {text}", "DialogueChoiceButton | OnMouseEnter");
        hoverIcon.style.display = DisplayStyle.Flex;
    }

    private void OnMouseLeave(MouseLeaveEvent evt)
    {
        SBGDebug.LogInfo($"Mouse left button: {text}", "DialogueChoiceButton | OnMouseLeave");
        hoverIcon.style.display = DisplayStyle.None;
    }
}