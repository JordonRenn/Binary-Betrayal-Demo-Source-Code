using UnityEngine;
using System;
using System.Linq;

public static class ObjectiveFactory
{
    public static IObjective CreateObjective(Objective objectiveData)
    {
        try
        {
            if (objectiveData == null)
            {
                SBGDebug.LogError("Objective data is null", "ObjectiveFactory");
                return null;
            }

            return objectiveData.type switch
            {
                ObjectiveType.Collect => ValidateAndCreate(() => new Objective_Collect(
                    objectiveData.objective_id,
                    objectiveData.item ?? new string[0],
                    objectiveData.quantity,
                    objectiveData.message), "Collect"),

                ObjectiveType.Kill => ValidateAndCreate(() => new Objective_Kill(
                    objectiveData.objective_id,
                    objectiveData.enemy ?? new string[0],
                    objectiveData.quantity,
                    objectiveData.message), "Kill"),

                ObjectiveType.Talk => ValidateAndCreate(() => new Objective_Talk(
                    objectiveData.objective_id,
                    objectiveData.dialogID,
                    objectiveData.message), "Talk"),

                ObjectiveType.Interact => ValidateAndCreate(() => new Objective_Interact(
                    objectiveData.objective_id,
                    objectiveData.target?.Select(t => t.targetId).ToArray() ?? Array.Empty<string>(),
                    objectiveData.message), "Interact"),
                    
                ObjectiveType.DoorLock => ValidateAndCreate(() => new Objective_DoorLock(
                    objectiveData.objective_id,
                    objectiveData.target?.FirstOrDefault()?.targetId ?? string.Empty,
                    objectiveData.message), "DoorLock"),

                ObjectiveType.PhoneCall => ValidateAndCreate(() => new Objective_PhoneCall(
                    objectiveData.objective_id,
                    objectiveData.target?.FirstOrDefault()?.targetId ?? string.Empty,
                    objectiveData.message), "PhoneCall"),

                ObjectiveType.Explore => ValidateAndCreate(() => new Objective_Explore(
                    objectiveData.objective_id,
                    objectiveData.target?.FirstOrDefault()?.targetId ?? string.Empty,
                    objectiveData.message), "Explore"),

                ObjectiveType.UseItem => ValidateAndCreate(() => new Objective_UseItem(
                    objectiveData.objective_id,
                    objectiveData.item?.FirstOrDefault() ?? string.Empty,
                    objectiveData.message), "UseItem"),

                _ => throw new System.ArgumentException($"Unsupported objective type: {objectiveData.type}")
            };
        }
        catch (System.Exception ex)
        {
            SBGDebug.LogException(ex, $"ObjectiveFactory.CreateObjective failed for type {objectiveData?.type}");
            return null;
        }
    }

    private static IObjective ValidateAndCreate(System.Func<IObjective> creator, string objectiveType)
    {
        try
        {
            var objective = creator();
            if (objective == null)
            {
                SBGDebug.LogError($"Failed to create {objectiveType} objective", "ObjectiveFactory");
                return null;
            }
            return objective;
        }
        catch (System.Exception ex)
        {
            SBGDebug.LogException(ex, $"ObjectiveFactory.ValidateAndCreate failed for {objectiveType}");
            return null;
        }
    }
}
