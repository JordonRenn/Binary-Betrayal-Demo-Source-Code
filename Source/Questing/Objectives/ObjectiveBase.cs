using UnityEngine;

namespace SBG.Questing
{
    public interface IObjective
    {
        int ObjectiveID { get; }
        ObjectiveType Type { get; }
        string Message { get; }
        bool IsCompleted { get; }
        ObjectiveStatus Status { get; }
        float Progress { get; }
        void MarkAsComplete(bool success = true);
        void ForceComplete(ObjectiveStatus status);
    }
    
    [System.Serializable]
    public abstract class ObjectiveBase : IObjective
    {
        public int ObjectiveID { get; protected set; }
        public abstract ObjectiveType Type { get; }
        public string Message { get; protected set; }
        public bool IsCompleted { get; protected set; }
        public ObjectiveStatus Status { get; protected set; }
        public float Progress { get; protected set; }

        protected ObjectiveBase(int objectiveId, string message)
        {
            ObjectiveID = objectiveId;
            Message = message;
            IsCompleted = false;
            Status = ObjectiveStatus.InProgress;
            Progress = 0f;
        }

        public virtual void MarkAsComplete(bool success = true)
        {
            if (!IsCompleted)
            {
                IsCompleted = true;
                Progress = 1f;
                Status = success ? ObjectiveStatus.CompletedSuccess : ObjectiveStatus.CompletedFailure;
                // SBGDebug.LogInfo($"Objective {ObjectiveID} completed with status {Status}: {Message}", GetType().Name);
            }
        }

        public virtual void ForceComplete(ObjectiveStatus status)
        {
            IsCompleted = true;
            Progress = 1f;
            Status = status;
            // SBGDebug.LogInfo($"Objective {ObjectiveID} force completed with status {Status}: {Message}", GetType().Name);
        }

        protected void UpdateProgress(float currentValue, float targetValue)
        {
            Progress = Mathf.Clamp01(currentValue / targetValue);
            if (Mathf.Approximately(Progress, 1f))
            {
                MarkAsComplete();
            }
        }
    }
}