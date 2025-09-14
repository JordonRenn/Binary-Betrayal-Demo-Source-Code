using System.Collections.Generic;
using System.Linq;
using DialogueSystem.Data;

namespace DialogueSystem.Graph
{
    public static class DialogueGraphConverter
    {
        public static DialogueData ConvertToRuntimeFormat(DialogueGraphData graphData)
        {
            if (graphData == null) return null;

            var dialogueData = new DialogueData
            {
                dialogueId = graphData.dialogueId,
                freezePlayer = graphData.freezePlayer,
                entries = new List<DialogueEntry>()
            };

            // Convert each node to a dialogue entry
            foreach (var nodeData in graphData.nodes)
            {
                DialogueEntry entry;
                
                switch (nodeData.type)
                {
                    case DialogueType.Choice:
                        var choiceEntry = new DialogueEntryChoice
                        {
                            type = DialogueType.Choice,
                            characterName = nodeData.characterName,
                            message = nodeData.message,
                            choices = new List<Choice>()
                        };

                        // Convert choices
                        if (nodeData.choices != null)
                        {
                            foreach (var choiceData in nodeData.choices)
                            {
                                var choice = new Choice
                                {
                                    text = choiceData.text,
                                    choiceType = choiceData.choiceType,
                                    nextDialogueId = choiceData.nextDialogueId
                                };

                                // Handle different choice types
                                if (!string.IsNullOrEmpty(choiceData.itemId))
                                {
                                    if (choiceData.choiceType == ChoiceType.GiveItem)
                                    {
                                        choice = new ChoiceGiveItem
                                        {
                                            text = choiceData.text,
                                            itemId = choiceData.itemId,
                                            quantity = choiceData.quantity,
                                            nextDialogueId = choiceData.nextDialogueId
                                        };
                                    }
                                    else if (choiceData.choiceType == ChoiceType.TakeItem)
                                    {
                                        choice = new ChoiceTakeItem
                                        {
                                            text = choiceData.text,
                                            itemId = choiceData.itemId,
                                            quantity = choiceData.quantity,
                                            nextDialogueId = choiceData.nextDialogueId
                                        };
                                    }
                                }
                                else if (!string.IsNullOrEmpty(choiceData.questId))
                                {
                                    choice = new ChoiceStartQuest
                                    {
                                        text = choiceData.text,
                                        questId = choiceData.questId,
                                        nextDialogueId = choiceData.nextDialogueId
                                    };
                                }

                                // Find the connected node through connections
                                var connection = graphData.connections.FirstOrDefault(c => 
                                    c.outputNodeId == nodeData.id && 
                                    c.outputPortName == $"Choice {choiceData.outputNodeId} Output");

                                if (connection != null)
                                {
                                    var connectedNode = graphData.nodes.FirstOrDefault(n => n.id == connection.inputNodeId);
                                    if (connectedNode != null)
                                    {
                                        // Set the index of the connected node in the entries list
                                        choice.nextEntryIndex = graphData.nodes.IndexOf(connectedNode);
                                    }
                                }

                                choiceEntry.choices.Add(choice);
                            }
                        }
                        entry = choiceEntry;
                        break;

                    case DialogueType.LoadNewDialogue:
                        entry = new DialogueEntryLoadNewDialogue
                        {
                            type = DialogueType.LoadNewDialogue,
                            characterName = nodeData.characterName,
                            message = nodeData.message,
                            nextDialogueId = nodeData.nextDialogueId
                        };
                        break;

                    default: // DialogueType.Text
                        entry = new DialogueEntry
                        {
                            type = DialogueType.Text,
                            characterName = nodeData.characterName,
                            message = nodeData.message
                        };
                        break;
                }

                dialogueData.entries.Add(entry);
            }

            return dialogueData;
        }
    }
}