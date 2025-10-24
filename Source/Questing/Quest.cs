using System.Collections.Generic;
using SBG.Questing.Data;

namespace SBG.Questing
{
    public abstract class Quest
    {
        public string ID { get; set; }
        public QuestType Type { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public List<IObjective> Objectives { get; protected set; }
        public QuestState CurrentState { get; protected set; }

        public virtual void Start()
        {
            // Initialize the quest
            CurrentState = QuestState.Incomplete;
        }

        public virtual bool IsCompleted() => CurrentState == QuestState.Complete;

        public List<string> Prerequisites { get; set; }

        public virtual void Complete()
        {
            if (CurrentState != QuestState.Complete)
            {
                CurrentState = QuestState.Complete;
                SBGDebug.LogInfo($"Completed quest: {Title}", GetType().Name);
            }
        }

        public void CheckPrerequisites()
        {
            // Logic to check if prerequisites are met
        }
    }
}