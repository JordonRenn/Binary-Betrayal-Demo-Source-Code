using System;
using System.Collections.Generic;

[Serializable]
public class DialogueData
{
    public string dialogueId;
    public bool freezePlayer = false;
    public List<DialogueEntry> entries;
}

public enum DialogueType
{
    Text,
    Choice,
    LoadNewDialogue
}

public enum ChoiceType
{
    Dialogue,
    GiveItem,
    TakeItem,
    StartQuest
}

[Serializable]
public class DialogueEntry
{
    public DialogueType type = DialogueType.Text;
    public string characterName;
    public string avatarPath;
    public string message;

    public DialogueEntry()
    {
        characterName = string.Empty;
        avatarPath = string.Empty;
        message = string.Empty;
    }
}

[Serializable]
public class DialogueEntryChoice : DialogueEntry
{
    public List<Choice> choices;

    public DialogueEntryChoice()
    {
        type = DialogueType.Choice; // Set the inherited type field
        choices = new List<Choice>();
        characterName = string.Empty;
        avatarPath = string.Empty;
        message = string.Empty;
    }
}

[Serializable]
public class DialogueEntryLoadNewDialogue : DialogueEntry
{
    public string nextDialogueId; // This one still needs dialogueId because it actually loads a new file

    public DialogueEntryLoadNewDialogue()
    {
        type = DialogueType.LoadNewDialogue; // Set the inherited type field
        characterName = string.Empty;
        avatarPath = string.Empty;
        message = string.Empty;
        nextDialogueId = string.Empty;
    }
}

[Serializable]
public class Choice
{
    public ChoiceType choiceType = ChoiceType.Dialogue;
    public string text;
    public int nextEntryIndex; // Index in the entries array instead of a file ID
    public string nextDialogueId; // ID of the next dialogue file if loading a new dialogue

    public Choice()
    {
        text = string.Empty;
        nextEntryIndex = -1;
        nextDialogueId = string.Empty;
    }
}

[Serializable]
public class ChoiceGiveItem : Choice
{
    public string itemId;
    public int quantity;

    public ChoiceGiveItem()
    {
        choiceType = ChoiceType.GiveItem;
        text = string.Empty;
        nextEntryIndex = -1;
        itemId = string.Empty;
        quantity = 0;
    }
}

[Serializable]
public class ChoiceTakeItem : Choice
{
    public string itemId;
    public int quantity;

    public ChoiceTakeItem()
    {
        choiceType = ChoiceType.TakeItem;
        text = string.Empty;
        nextEntryIndex = -1;
        itemId = string.Empty;
        quantity = 0;
    }
}

[Serializable]
public class ChoiceStartQuest : Choice
{
    public string questId;

    public ChoiceStartQuest()
    {
        choiceType = ChoiceType.StartQuest;
        text = string.Empty;
        nextEntryIndex = -1;
        questId = string.Empty;
    }
}