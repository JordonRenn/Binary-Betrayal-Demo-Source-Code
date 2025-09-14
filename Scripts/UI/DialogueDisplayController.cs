using UnityEngine;
using DialogueSystem;
using DialogueSystem.Data;
using DialogueSystem.Graph;
using UnityEngine.UIElements;
using System.Collections.Generic;

/// <summary>
/// Simplified dialogue display controller that handles text and choice dialogues
/// </summary>
public class DialogueDisplayController : MonoBehaviour
{
    #region Singleton

    private static DialogueDisplayController _instance;
    public static DialogueDisplayController Instance
    {
        get
        {
            if (_instance == null)
            {
                SBGDebug.LogError("Attempting to access DialogueDisplayController before it is initialized.", "DialogueDisplayController");
            }
            return _instance;
        }
        private set => _instance = value;
    }

    #endregion

    #region Fields

    [Header("UI References")]
    [SerializeField] private UIDocument dialogueUIDocument;

    // UI Element References
    private VisualElement dialogueBox;
    private Label dialogueLabel;
    private VisualElement choiceBox;
    private VisualElement choiceListView;

    // Current dialogue state
    private DialogueData currentDialogue;
    private int currentEntryIndex;
    private List<DialogueChoiceButton> choiceButtons = new List<DialogueChoiceButton>();

    #endregion

    #region Properties

    public bool IsDialogueActive => currentDialogue != null && currentEntryIndex < currentDialogue.entries.Count;
    public DialogueEntry CurrentEntry => IsDialogueActive ? currentDialogue.entries[currentEntryIndex] : null;

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        if (this.InitializeSingleton(ref _instance, true) == this)
        {
            currentEntryIndex = -1;
        }
    }

    private void Start()
    {
        InitializeUIElements();

        // Subscribe to game events
        if (GameMaster.Instance != null)
        {
            GameMaster.Instance.gm_DialogueStarted?.AddListener(OnDialogueStartedEvent);
            GameMaster.Instance.gm_DialogueEnded?.AddListener(OnDialogueEndedEvent);
        }

        // Subscribe to input events for dialogue advancement
        if (InputHandler.Instance != null)
        {
            InputHandler.Instance.OnInteractInput.AddListener(OnInteractInputReceived);
            InputHandler.Instance.OnFocus_InteractInput.AddListener(OnInteractInputReceived);
            InputHandler.Instance.OnUI_SubmitInput.AddListener(OnInteractInputReceived);
            InputHandler.Instance.OnUI_ClickInput.AddListener(OnInteractInputReceived);
            InputHandler.Instance.OnUI_InteractInput.AddListener(OnInteractInputReceived);
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        if (GameMaster.Instance != null)
        {
            GameMaster.Instance.gm_DialogueStarted?.RemoveListener(OnDialogueStartedEvent);
            GameMaster.Instance.gm_DialogueEnded?.RemoveListener(OnDialogueEndedEvent);
        }

        if (InputHandler.Instance != null)
        {
            InputHandler.Instance.OnInteractInput.RemoveListener(OnInteractInputReceived);
            InputHandler.Instance.OnFocus_InteractInput.RemoveListener(OnInteractInputReceived);
            InputHandler.Instance.OnUI_SubmitInput.RemoveListener(OnInteractInputReceived);
            InputHandler.Instance.OnUI_ClickInput.RemoveListener(OnInteractInputReceived);
            InputHandler.Instance.OnUI_InteractInput.RemoveListener(OnInteractInputReceived);
        }
    }

    #endregion

    #region Public API

    /// <summary>
    /// Start a dialogue with the given dialogue ID
    /// </summary>
    public bool StartDialogue(string dialogueId)
    {
        if (string.IsNullOrEmpty(dialogueId))
        {
            SBGDebug.LogError("Cannot start dialogue with null or empty ID", "DialogueDisplayController | StartDialogue");
            return false;
        }

        var dialogueData = DialogueLoader.LoadDialogue(dialogueId);
        if (dialogueData == null)
        {
            SBGDebug.LogError($"Failed to load dialogue: {dialogueId}", "DialogueDisplayController | StartDialogue");
            return false;
        }

        currentDialogue = dialogueData;
        currentEntryIndex = 0;

        GameMaster.Instance?.gm_DialogueStarted?.Invoke();
        DisplayCurrentEntry();

        SBGDebug.LogInfo($"Started dialogue: {dialogueId}", "DialogueDisplayController | StartDialogue");
        return true;
    }

    /// <summary>
    /// End the current dialogue
    /// </summary>
    public void EndDialogue()
    {
        if (!IsDialogueActive) return;

        SetDialogueBoxVisible(false);
        SetChoiceBoxVisible(false);
        ClearChoiceButtons();

        currentDialogue = null;
        currentEntryIndex = -1;

        GameMaster.Instance?.gm_DialogueEnded?.Invoke();

        SBGDebug.LogInfo("Ended dialogue", "DialogueDisplayController | EndDialogue");
    }

    /// <summary>
    /// Advance to the next dialogue entry (for text entries)
    /// </summary>
    public void AdvanceDialogue()
    {
        if (!IsDialogueActive) return;

        var currentEntry = CurrentEntry;
        if (currentEntry == null)
        {
            EndDialogue();
            return;
        }

        // Only advance for text and LoadNewDialogue entries, not choices
        if (currentEntry.type == DialogueType.Text)
        {
            currentEntryIndex++;
            if (currentEntryIndex >= currentDialogue.entries.Count)
            {
                EndDialogue();
            }
            else
            {
                DisplayCurrentEntry();
            }
        }
        else if (currentEntry.type == DialogueType.LoadNewDialogue)
        {
            var loadEntry = currentEntry as DialogueEntryLoadNewDialogue;
            if (!string.IsNullOrEmpty(loadEntry?.nextDialogueId))
            {
                StartDialogue(loadEntry.nextDialogueId);
            }
            else
            {
                EndDialogue();
            }
        }
    }

    /// <summary>
    /// Select a choice in a choice dialogue entry
    /// </summary>
    public void SelectChoice(int choiceIndex)
    {
        if (!IsDialogueActive) return;

        var currentEntry = CurrentEntry as DialogueEntryChoice;
        if (currentEntry?.choices == null || choiceIndex < 0 || choiceIndex >= currentEntry.choices.Count)
        {
            SBGDebug.LogError($"Invalid choice index: {choiceIndex}", "DialogueDisplayController | SelectChoice");
            return;
        }

        var selectedChoice = currentEntry.choices[choiceIndex];

        // Handle choice effects if any
        // TODO: Add choice effects handling if needed

        // Check if choice leads to new dialogue or ends
        if (!string.IsNullOrEmpty(selectedChoice.nextDialogueId))
        {
            StartDialogue(selectedChoice.nextDialogueId);
        }
        else if (selectedChoice.nextEntryIndex >= 0)
        {
            currentEntryIndex = selectedChoice.nextEntryIndex;
            if (currentEntryIndex >= currentDialogue.entries.Count)
            {
                EndDialogue();
            }
            else
            {
                DisplayCurrentEntry();
            }
        }
        else
        {
            EndDialogue();
        }
    }

    #endregion

    #region Private Methods

    private void InitializeUIElements()
    {
        if (dialogueUIDocument == null)
        {
            SBGDebug.LogError("DialogueUIDocument is not assigned", "DialogueDisplayController | InitializeUIElements");
            return;
        }

        var root = dialogueUIDocument.rootVisualElement;

        dialogueBox = root.Q<VisualElement>("DialogueBox");
        dialogueLabel = root.Q<Label>(className: "dialogue-box-label");
        choiceBox = root.Q<VisualElement>("ChoiceBox");
        
        // Get the ListView inside ChoiceBox and treat it as a regular VisualElement for button layout
        choiceListView = choiceBox?.Q<VisualElement>("ListView");

        if (dialogueBox == null || dialogueLabel == null || choiceBox == null || choiceListView == null)
        {
            SBGDebug.LogError("Failed to find required UI elements", "DialogueDisplayController | InitializeUIElements");
            return;
        }

        // Set up choice list view for vertical button layout
        choiceListView.style.flexDirection = FlexDirection.Column;
        choiceListView.style.alignItems = Align.Center;
        choiceListView.style.justifyContent = Justify.Center;

        SetDialogueBoxVisible(false);
        SetChoiceBoxVisible(false);

        // Add click handler for advancing dialogue
        dialogueBox.RegisterCallback<ClickEvent>(OnDialogueBoxClicked);

        SBGDebug.LogInfo("UI elements initialized successfully", "DialogueDisplayController | InitializeUIElements");
    }

    private void OnDialogueBoxClicked(ClickEvent evt)
    {
        if (!IsDialogueActive) return;

        var currentEntry = CurrentEntry;
        if (currentEntry != null && currentEntry.type != DialogueType.Choice)
        {
            AdvanceDialogue();
        }
    }

    private void OnInteractInputReceived()
    {
        if (!IsDialogueActive) return;

        var currentEntry = CurrentEntry;
        if (currentEntry != null && currentEntry.type != DialogueType.Choice)
        {
            AdvanceDialogue();
        }
    }

    private void SetDialogueBoxVisible(bool visible)
    {
        if (dialogueBox != null)
        {
            dialogueBox.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }

    private void SetChoiceBoxVisible(bool visible)
    {
        if (choiceBox != null)
        {
            choiceBox.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }

    private void ClearChoiceButtons()
    {
        foreach (var button in choiceButtons)
        {
            if (button?.parent != null)
            {
                button.parent.Remove(button);
            }
        }
        choiceButtons.Clear();
    }

    private void DisplayCurrentEntry()
    {
        var currentEntry = CurrentEntry;
        if (currentEntry == null)
        {
            EndDialogue();
            return;
        }

        switch (currentEntry.type)
        {
            case DialogueType.Text:
                DisplayTextEntry(currentEntry);
                break;

            case DialogueType.Choice:
                DisplayChoiceEntry(currentEntry as DialogueEntryChoice);
                break;

            case DialogueType.LoadNewDialogue:
                DisplayLoadNewDialogueEntry(currentEntry as DialogueEntryLoadNewDialogue);
                break;
        }
    }

    private void DisplayTextEntry(DialogueEntry entry)
    {
        if (dialogueLabel == null) return;

        string displayText = entry.message;
        if (!string.IsNullOrEmpty(entry.characterName))
        {
            displayText = $"{entry.characterName}: {entry.message}";
        }

        dialogueLabel.text = displayText;

        SetDialogueBoxVisible(true);
        SetChoiceBoxVisible(false);
        ClearChoiceButtons();

        SBGDebug.LogInfo($"Displaying text entry: {entry.characterName}: {entry.message}", "DialogueDisplayController | DisplayTextEntry");
    }

    private void DisplayChoiceEntry(DialogueEntryChoice choiceEntry)
    {
        if (choiceEntry?.choices == null || choiceEntry.choices.Count == 0)
        {
            SBGDebug.LogError("Choice entry has no choices", "DialogueDisplayController | DisplayChoiceEntry");
            EndDialogue();
            return;
        }

        if (dialogueLabel == null) return;

        // Display the dialogue text
        string displayText = choiceEntry.message;
        if (!string.IsNullOrEmpty(choiceEntry.characterName))
        {
            displayText = $"{choiceEntry.characterName}: {choiceEntry.message}";
        }
        dialogueLabel.text = displayText;

        // Clear existing choice buttons
        ClearChoiceButtons();

        // Create new choice buttons
        for (int i = 0; i < choiceEntry.choices.Count; i++)
        {
            var choice = choiceEntry.choices[i];
            var choiceButton = new DialogueChoiceButton();
            choiceButton.choiceText = choice.text;

            // Store the choice index for the click handler
            int choiceIndex = i; // Capture for closure
            
            // Use Button's built-in clicked event instead of ClickEvent
            choiceButton.clicked += () => {
                SBGDebug.LogInfo($"Choice button clicked: {choiceIndex}", "DialogueDisplayController | DisplayChoiceEntry");
                SelectChoice(choiceIndex);
            };

            // Add debug logging for mouse events (these should work now)
            choiceButton.RegisterCallback<MouseEnterEvent>(evt => {
                SBGDebug.LogInfo($"Mouse entered choice button {choiceIndex}", "DialogueDisplayController | DisplayChoiceEntry");
            });

            choiceButton.RegisterCallback<MouseLeaveEvent>(evt => {
                SBGDebug.LogInfo($"Mouse left choice button {choiceIndex}", "DialogueDisplayController | DisplayChoiceEntry");
            });

            // Add styling for centered, vertically stacked buttons
            choiceButton.style.alignSelf = Align.Center;
            choiceButton.style.marginBottom = 5; // Small gap between buttons

            choiceListView.Add(choiceButton);
            choiceButtons.Add(choiceButton);
            
            SBGDebug.LogInfo($"Added choice button {choiceIndex}: {choice.text} - Parent: {choiceButton.parent?.name}", "DialogueDisplayController | DisplayChoiceEntry");
        }

        // Debug the hierarchy
        SBGDebug.LogInfo($"ChoiceListView children count: {choiceListView.childCount}", "DialogueDisplayController | DisplayChoiceEntry");
        SBGDebug.LogInfo($"ChoiceBox children count: {choiceBox.childCount}", "DialogueDisplayController | DisplayChoiceEntry");

        // Show both dialogue and choice boxes
        SetDialogueBoxVisible(true);
        SetChoiceBoxVisible(true);

        SBGDebug.LogInfo($"Displaying choice entry: {choiceEntry.characterName}: {choiceEntry.message} ({choiceEntry.choices.Count} choices)",
            "DialogueDisplayController | DisplayChoiceEntry");
    }

    private void DisplayLoadNewDialogueEntry(DialogueEntryLoadNewDialogue loadEntry)
    {
        if (dialogueLabel == null) return;

        string displayText = loadEntry.message;
        if (!string.IsNullOrEmpty(loadEntry.characterName))
        {
            displayText = $"{loadEntry.characterName}: {loadEntry.message}";
        }

        dialogueLabel.text = displayText;

        SetDialogueBoxVisible(true);
        SetChoiceBoxVisible(false);
        ClearChoiceButtons();

        SBGDebug.LogInfo($"Displaying load dialogue entry: {loadEntry.characterName}: {loadEntry.message} (loads: {loadEntry.nextDialogueId})",
            "DialogueDisplayController | DisplayLoadNewDialogueEntry");
    }

    #endregion

    #region Event Handlers
    private void OnDialogueStartedEvent()
    {
        SBGDebug.LogInfo("Dialogue started event received", "DialogueDisplayController | OnDialogueStartedEvent");
    }

    private void OnDialogueEndedEvent()
    {
        SBGDebug.LogInfo("Dialogue ended event received", "DialogueDisplayController | OnDialogueEndedEvent");
    }
    #endregion
}