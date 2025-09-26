using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/* 
- WeaponData scriptable objects are named based on WeaponID enum declarations in SBG.cs
- The file paths for these assets are constructed using the WeaponID names.
 */

#region WEAPON INVENTORY
public static class WeaponInventory
{
    private static string PATH_WEAPON_DATA = Path.Combine(Application.dataPath, "Data/WeaponData");
    private static string PATH_WEAPON_INVENTORY = Path.Combine(Application.streamingAssetsPath, "Inventories");
    private const string FILE_INVENTORY = "WeaponInventory.json";

    private static List<WeaponID> weaponInventory = new List<WeaponID>();

    public static bool inventoryLoaded = false;

    static WeaponInventory()
    {
        LoadWeaponInventory();
    }

    #region Load Inventory
    private static void LoadWeaponInventory()
    {
        string filePath = Path.Combine(PATH_WEAPON_INVENTORY, FILE_INVENTORY);

        try
        {
            if (File.Exists(filePath))
            {
                string jsonData = File.ReadAllText(filePath);
                WeaponInventoryData inventoryData = JsonUtility.FromJson<WeaponInventoryData>(jsonData);

                ClearInventory();

                // Convert string IDs to enum values and add them to inventory
                foreach (string weaponIDString in inventoryData.weaponIDs)
                {
                    if (Enum.TryParse(weaponIDString, out WeaponID weaponID))
                    {
                        AddWeapon(weaponID);
                    }
                    else
                    {
                        SBGDebug.LogWarning($"Failed to parse weapon ID: {weaponIDString}", "WeaponInventory | LoadInventory");
                    }
                }

                // SBGDebug.LogInfo($"Loaded {weaponInventory.Count} weapons to inventory", "WeaponInventory | LoadInventory");
            }
            else
            {
                SBGDebug.LogError($"Weapon inventory file not found at {filePath}", "WeaponInventory | LoadInventory");
            }
        }
        catch (Exception e)
        {
            SBGDebug.LogError($"Error loading weapon inventory: {e.Message}", "WeaponInventory | LoadInventory");
        }

        inventoryLoaded = true;
    }
    #endregion

    #region Save Inventory
    public static bool SaveInventory()
    {
        try
        {
            // Create inventory data from current weapon inventory
            WeaponInventoryData inventoryData = new WeaponInventoryData();
            foreach (WeaponID weaponID in weaponInventory)
            {
                inventoryData.weaponIDs.Add(weaponID.ToString());
            }

            // Convert to JSON
            string jsonData = JsonUtility.ToJson(inventoryData, true);
            string filePath = Path.Combine(PATH_WEAPON_INVENTORY, FILE_INVENTORY);

            // Ensure directory exists
            Directory.CreateDirectory(PATH_WEAPON_INVENTORY);

            // Write to file
            File.WriteAllText(filePath, jsonData);

            // SBGDebug.LogInfo($"Saved {weaponInventory.Count} weapons to inventory file at {filePath}", "WeaponInventory | SaveInventory");
            return true;
        }
        catch (Exception e)
        {
            SBGDebug.LogError($"Error saving weapon inventory: {e.Message}", "WeaponInventory | SaveInventory");
            return false;
        }
    }

    #endregion

    #region PUBLIC METHODS
    public static List<WeaponData> GetCurrentWeaponDatas()
    {
        List<WeaponData> weaponDataCache = new List<WeaponData>();

        foreach (var weaponID in weaponInventory)
        {
            // Use Resources.Load with path relative to Resources folder
            // Assuming your WeaponData assets are in Assets/Resources/WeaponData/
            string resourcePath = "WeaponData/" + weaponID.ToString();
            WeaponData weaponData = Resources.Load<WeaponData>(resourcePath);
            
            if (weaponData != null)
            {
                weaponDataCache.Add(weaponData);
            }
            else
            {
                SBGDebug.LogError($"Failed to load WeaponData for {weaponID}. Make sure the asset is in Resources/WeaponData/ folder.", "WeaponInventory | GetCurrentWeaponDatas");
            }
        }

        return weaponDataCache;
    }



    public static void AddWeapon(WeaponID weaponID)
    {
        if (!weaponInventory.Contains(weaponID))
        {
            weaponInventory.Add(weaponID);
        }
    }

    public static void RemoveWeapon(WeaponID weaponID)
    {
        if (weaponInventory.Contains(weaponID))
        {
            weaponInventory.Remove(weaponID);
        }
    }

    public static bool HasWeapon(WeaponID weaponID)
    {
        return weaponInventory.Contains(weaponID);
    }

    public static void ClearInventory()
    {
        weaponInventory.Clear();
    }
    #endregion
}
#endregion

#region WeaponInventoryData
[Serializable]
public class WeaponInventoryData
{
    public List<string> weaponIDs = new List<string>();
}
#endregion