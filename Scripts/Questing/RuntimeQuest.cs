using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class RuntimeQuest : Quest
{
    private QuestData data;

    public RuntimeQuest(QuestData questData)
    {
        data = questData;
        ID = questData.quest_id;
        Type = questData.type;  // Using the Type property from base class
        Title = questData.title;
        Description = questData.description;
        Objectives = questData.objectives.Select(obj => ObjectiveFactory.CreateObjective(obj)).ToList();
        CurrentState = QuestState.Incomplete;
    }

    public override void Start()
    {
        if (CurrentState == QuestState.Incomplete)
        {
            CurrentState = QuestState.InProgress;
            SBGDebug.LogInfo($"Started quest: {Title}", "RuntimeQuest");
        }
    }
}