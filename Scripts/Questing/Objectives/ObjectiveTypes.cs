using UnityEngine;
using System.Linq;
using System;

[System.Serializable]
public class Objective_Talk : ObjectiveBase
{
    private readonly string targetDialogId;
    
    public override ObjectiveType Type => ObjectiveType.Talk;

    public Objective_Talk(int objectiveId, string dialogId, string message) 
        : base(objectiveId, message)
    {
        targetDialogId = dialogId;
    }

    public void OnDialogueCompleted(string dialogId)
    {
        if (IsCompleted) return;
        
        if (dialogId == targetDialogId)
        {
            MarkAsComplete();
        }
    }
}

[System.Serializable]
public class Objective_Collect : ObjectiveBase
{
    private readonly string[] itemsToCollect;
    private readonly int requiredQuantity;
    private int currentQuantity;

    public override ObjectiveType Type => ObjectiveType.Collect;

    public Objective_Collect(int objectiveId, string[] items, int quantity, string message)
        : base(objectiveId, message)
    {
        itemsToCollect = items;
        requiredQuantity = quantity;
        currentQuantity = 0;
    }

    public void OnItemCollected(string itemId)
    {
        if (IsCompleted) return;

        if (itemsToCollect.Contains(itemId))
        {
            currentQuantity++;
            UpdateProgress(currentQuantity, requiredQuantity);
        }
    }
}

[System.Serializable]
public class Objective_DoorLock : ObjectiveBase
{
    public override ObjectiveType Type => ObjectiveType.DoorLock;

    public string DoorID { get; private set; }

    public Objective_DoorLock(int id, string doorId, string message)
        : base(id, message)
    {
        DoorID = doorId;
    }

    public void DoorUnlocked(string doorId)
    {
        if (doorId == DoorID)
        {
            MarkAsComplete();
        }
    }
}

[System.Serializable]
public class Objective_PhoneCall : ObjectiveBase
{
    public override ObjectiveType Type => ObjectiveType.PhoneCall;

    public string PhoneNumber { get; private set; }

    public Objective_PhoneCall(int id, string phoneNumber, string message)
        : base(id, message)
    {
        PhoneNumber = phoneNumber;
    }

    public void CallMade(string phoneNumber)
    {
        if (phoneNumber == PhoneNumber)
        {
            MarkAsComplete();
        }
    }
}

[System.Serializable]
public class Objective_Kill : ObjectiveBase
{
    private readonly string[] targetEnemies;
    private readonly int requiredKills;
    private int currentKills;

    public override ObjectiveType Type => ObjectiveType.Kill;

    public Objective_Kill(int objectiveId, string[] enemies, int quantity, string message) 
        : base(objectiveId, message)
    {
        targetEnemies = enemies;
        requiredKills = quantity;
        currentKills = 0;
    }

    public void OnEnemyKilled(string enemyId)
    {
        if (IsCompleted) return;
        
        if (targetEnemies.Contains(enemyId))
        {
            currentKills++;
            UpdateProgress(currentKills, requiredKills);
        }
    }
}

[System.Serializable]
public class Objective_Explore : ObjectiveBase
{
    public override ObjectiveType Type => ObjectiveType.Explore;

    public string AreaID { get; private set; }

    public Objective_Explore(int id, string areaId, string message)
        : base(id, message)
    {
        AreaID = areaId;
    }

    public void AreaExplored(string areaId)
    {
        if (areaId == AreaID)
        {
            MarkAsComplete();
        }
    }
}

[System.Serializable]
public class Objective_UseItem : ObjectiveBase
{
    public override ObjectiveType Type => ObjectiveType.UseItem;

    public string ItemID { get; private set; }

    public Objective_UseItem(int id, string itemId, string message)
        : base(id, message)
    {
        ItemID = itemId;
    }

    public void ItemUsed(string itemId)
    {
        if (itemId == ItemID)
        {
            MarkAsComplete();
        }
    }
}

[System.Serializable]
public class Objective_Interact : ObjectiveBase
{
    private readonly string[] targetIds;
    private readonly bool[] completedTargets;
    private int completedInteractions;

    public override ObjectiveType Type => ObjectiveType.Interact;

    public Objective_Interact(int objectiveId, string[] targets, string message) 
        : base(objectiveId, message)
    {
        targetIds = targets ?? Array.Empty<string>();
        completedTargets = new bool[targetIds.Length];
        completedInteractions = 0;
    }

    public void OnInteractionComplete(string targetId)
    {
        if (IsCompleted) return;

        for (int i = 0; i < targetIds.Length; i++)
        {
            if (targetIds[i] == targetId && !completedTargets[i])
            {
                completedTargets[i] = true;
                completedInteractions++;
                UpdateProgress(completedInteractions, targetIds.Length);
                break;
            }
        }
    }
}
