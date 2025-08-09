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

[System.Serializable]
public class Objective
{
    public int objective_id;
    public ObjectiveType type;
    public string[] item;  // For collect objectives
    public int quantity;
    public int currentQuantity;
    public string[] enemy; // For kill objectives
    public string dialogID; // For talk objectives
    public ObjectiveTarget[] target;  // Use a specific type (e.g., NPC or item) later
    public string message;
    public bool IsCompleted { get; private set; }

    public float Progress =>
        quantity > 0 ? Mathf.Clamp01((float)currentQuantity / quantity) :
        IsCompleted ? 1f : 0f;

    public void MarkAsComplete()
    {
        IsCompleted = true;
    }

    public void UpdateProgress(int newCount)
    {
        currentQuantity = Mathf.Min(newCount, quantity);
        if (currentQuantity >= quantity)
            MarkAsComplete();
    }
}

[System.Serializable]
public class ObjectiveTarget
{
    public string targetId;
    public Vector3 position;
    public TargetType type;
    public bool isInteracted;
    
    // Additional data can be stored in a serialized format
    public string data;
}