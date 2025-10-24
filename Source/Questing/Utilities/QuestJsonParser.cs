using UnityEngine;
using System.Text;
using System.Collections.Generic;
using System;
using SBG.Questing.Data;

namespace SBG.Questing.Tools
{
    public static unsafe class QuestJsonParser
    {
        /// <summary>
        /// Parses a TextAsset into QuestData array using SimdJsonInterop
        /// </summary>
        public static QuestData[] ParseQuestsData(TextAsset jsonAsset)
        {
            if (jsonAsset == null)
            {
                Debug.LogError("Quest JSON asset is null!");
                return new QuestData[0];
            }

            string json = jsonAsset.text;
            int questCount = SimdJsonInterop.get_array_length(json, "quests");
            if (questCount <= 0)
            {
                Debug.LogError("No quests found in JSON.");
                return new QuestData[0];
            }

            var questList = new List<QuestData>(questCount);
            for (int i = 0; i < questCount; i++)
            {
                var quest = new QuestData();
                quest.quest_id = SimdJsonInterop.GetArrayString(json, "quests", i, "quest_id");
                quest.type = (QuestType)SimdJsonInterop.get_array_int(json, "quests", i, "type");
                quest.title = SimdJsonInterop.GetArrayString(json, "quests", i, "title");
                quest.description = SimdJsonInterop.GetArrayString(json, "quests", i, "description");
                quest.isRepeatable = SimdJsonInterop.get_array_bool(json, "quests", i, "isRepeatable") == 1;
                // NEED TO UPDATE INTEROP / SIMDJSON EXPORTS.CPP TO IMPLEMENT
                //quest.timeLimit = (float)SimdJsonInterop.get_array_double(json, "quests", i, "timeLimit");

                // Parse objectives
                var objectives = new List<Objective>();
                string objectivesJson = SimdJsonInterop.GetArrayString(json, "quests", i, "objectives");
                if (!string.IsNullOrEmpty(objectivesJson))
                {
                    int objCount = SimdJsonInterop.get_array_length(objectivesJson, "");
                    for (int j = 0; j < objCount; j++)
                    {
                        var objective = new Objective();
                        objective.objective_id = SimdJsonInterop.get_array_int(objectivesJson, "", j, "objective_id");
                        objective.type = (ObjectiveType)SimdJsonInterop.get_array_int(objectivesJson, "", j, "type");
                        objective.message = SimdJsonInterop.GetArrayString(objectivesJson, "", j, "message");
                        objective.quantity = SimdJsonInterop.get_array_int(objectivesJson, "", j, "quantity");

                        // TODO: Parse item, enemy, dialogID, target arrays/fields as needed
                        objectives.Add(objective);
                    }
                }
                quest.objectives = objectives.ToArray();

                // TODO: Parse prerequisiteQuestIds, rewards, etc. similarly

                questList.Add(quest);
            }

            return questList.ToArray();
        }
    }
}