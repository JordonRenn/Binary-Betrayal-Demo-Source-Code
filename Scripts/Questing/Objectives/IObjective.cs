using UnityEngine;

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
