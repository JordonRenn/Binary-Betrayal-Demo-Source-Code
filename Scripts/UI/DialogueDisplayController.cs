using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System;
using GlobalEvents;

/// <summary>
/// Simplified dialogue display controller that handles text and choice dialogues
/// </summary>
public class DialogueDisplayController : MonoBehaviour
{
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

    [Header("UI References")]
    [SerializeField] private UIDocument dialogueUIDocument;

    // UI Element References
    private VisualElement dialogueBox;
    private Label dialogueLabel;
    private VisualElement choiceBox;
    private VisualElement choiceListView;

    // Current dialogue state
    private DialogueData currentDialogue;
    private string currentNodeId;
    private DialogueEntry CurrentEntry;
    private List<DialogueChoiceButton> choiceButtons = new List<DialogueChoiceButton>();

    // Input state tracking
    private InputState? previousInputState;

    public bool IsDialogueActive => currentDialogue != null && CurrentEntry != null;
    //public DialogueEntry CurrentEntry => IsDialogueActive ? dialogueEntryCache[dialogueEntryCache.Count - 1] : null;

    #region Unity Lifecycle

    private void Awake()
    {
        if (this.InitializeSingleton(ref _instance, true) == this)
        {
            //currentEntryIndex = -1;
            currentNodeId = null;
        }
    }

    private void Start()
    {
        InitializeUIElements();

        DialogueEvents.DialogueTriggered += StartDialogue;
        DialogueEvents.DialogueEnded += EndDialogue;

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
        DialogueEvents.DialogueTriggered -= StartDialogue;
        DialogueEvents.DialogueEnded -= EndDialogue;

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
    public void StartDialogue(string dialogueId)
    {
        if (string.IsNullOrEmpty(dialogueId))
        {
            SBGDebug.LogError("Cannot start dialogue with null or empty ID", "DialogueDisplayController | StartDialogue");
            return;
        }

        var dialogueData = DialogueLoader.LoadDialogue(dialogueId);
        if (dialogueData == null)
        {
            SBGDebug.LogError($"Failed to load dialogue: {dialogueId}", "DialogueDisplayController | StartDialogue");
            return;
        }

        currentDialogue = dialogueData;
        CurrentEntry = dialogueData.entries[0];
        currentNodeId = CurrentEntry.nodeId;

        // GameMaster.Instance?.gm_DialogueStarted?.Invoke();
        DialogueEvents.RaiseDialogueStarted();
        DisplayEntry(CurrentEntry);
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
        CurrentEntry = null;
        currentNodeId = null;

        // Restore previous input state if we switched to UI for choice dialogue
        if (previousInputState.HasValue)
        {
            if (previousInputState != InputState.UI) InputHandler.Instance.SetInputState(previousInputState.Value);
            SBGDebug.LogInfo($"Restored input state to {previousInputState}", "DialogueDisplayController | EndDialogue");
            previousInputState = null;
        }

        // GameMaster.Instance?.gm_DialogueEnded?.Invoke();
        DialogueEvents.RaiseDialogueEnded();

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

        if (currentEntry.type == DialogueType.Choice)
        {
            return;
        }

        if (!string.IsNullOrEmpty(currentEntry.outputNodeId))
        {
            // load output node id
            currentNodeId = currentEntry.outputNodeId;
            var nextEntry = currentDialogue.entries.Find(e => e.nodeId == currentNodeId);
            DisplayEntry(nextEntry);
        }
        else
        {
            EndDialogue();
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

        if (previousInputState != InputState.UI) InputHandler.Instance.SetInputState(previousInputState.Value);

        previousInputState = null;

        if (!string.IsNullOrEmpty(selectedChoice.outputNodeId))
        {
            currentNodeId = selectedChoice.outputNodeId;
            var nextEntry = currentDialogue.entries.Find(e => e.nodeId == currentNodeId);
            DisplayEntry(nextEntry);
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

    private void DisplayEntry(DialogueEntry entry)
    {
        if (entry == null) { EndDialogue(); return; } 

        CurrentEntry = entry;

        switch (entry.type)
        {
            case DialogueType.Text:
                DisplayTextEntry(entry);
                break;

            case DialogueType.Choice:
                DisplayChoiceEntry(entry as DialogueEntryChoice);
                break;
        }
    }

    #region Display Text
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
    #endregion

    #region Display Choice
    private void DisplayChoiceEntry(DialogueEntryChoice choiceEntry)
    {
        if (choiceEntry?.choices == null || choiceEntry.choices.Count == 0)
        {
            SBGDebug.LogError("Choice entry has no choices", "DialogueDisplayController | DisplayChoiceEntry");
            EndDialogue();
            return;
        }

        if (dialogueLabel == null) return;

        // Cache current input state if not already UI and switch to UI state
        if (InputHandler.Instance.currentState != InputState.UI)
        {
            previousInputState = InputHandler.Instance.currentState;
            InputHandler.Instance.SetInputState(InputState.UI);
            SBGDebug.LogInfo($"Switched to UI input state (from {previousInputState})", "DialogueDisplayController | DisplayChoiceEntry");
        }

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

            int choiceIndex = i;

            // Use Button's built-in clicked event instead of ClickEvent
            choiceButton.clicked += () =>
            {
                SBGDebug.LogInfo($"Choice button clicked: {choiceIndex}", "DialogueDisplayController | DisplayChoiceEntry");
                SelectChoice(choiceIndex);
            };

            choiceButton.style.alignSelf = Align.Center;
            choiceButton.style.marginBottom = 5; // Small gap between buttons

            choiceListView.Add(choiceButton);
            choiceButtons.Add(choiceButton);
        }

        SetDialogueBoxVisible(true);
        SetChoiceBoxVisible(true);
    }
    #endregion

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