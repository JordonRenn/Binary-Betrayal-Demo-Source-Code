using System;
using System.Collections.Generic;

namespace SBG.DialogueSystem.Data
{
    [Serializable]
    public enum DialogueType
    {
        Text,
        Choice,
        LoadNewDialogue
    }

    [Serializable]
    public enum ChoiceType
    {
        Dialogue,
        GiveItem,
        TakeItem,
        StartQuest
    }

    [Serializable]
    public class DialogueData
    {
        public string dialogueId;
        public bool freezePlayer = false;
        public List<DialogueEntry> entries;
    }

    [Serializable]
    public class DialogueEntry
    {
        public DialogueType type = DialogueType.Text;
        public string nodeId;
        public string outputNodeId; // this is the next entry index key, can be null to end dialogue or if its a choice node
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
        public string outputNodeId; // The ID of the node this choice connects to // USE FOR NEXT ENTRY
        public string text;
        public int nextEntryIndex;

        public Choice()
        {
            text = string.Empty;
            nextEntryIndex = -1;
            outputNodeId = string.Empty;
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
            outputNodeId = string.Empty;
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
            outputNodeId = string.Empty;
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
            outputNodeId = string.Empty;
            questId = string.Empty;
        }
    }
}