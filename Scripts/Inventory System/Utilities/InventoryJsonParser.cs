using UnityEngine;
using System.Collections.Generic;
using System;

public static class InventoryJsonParser
{
    /// <summary>
    /// Parses inventory JSON and returns InventoryContextData with a list of ItemContextData (itemId, quantity).
    /// </summary>
    public static InventoryContextData ParseInventoryJsonData(TextAsset jsonTextAsset)
    {
        if (jsonTextAsset == null)
        {
            SBGDebug.LogError("Inventory JSON asset is null!", "InventoryJsonParser | ParseInventoryJsonData");
            return null;
        }

        try
        {
            string json = jsonTextAsset.text;
            // SBGDebug.LogInfo($"Parsing inventory. Asset name: {jsonTextAsset.name}", "InventoryJsonParser | ParseInventoryContext");

            InventoryContextData contextData = new InventoryContextData
            {
                InventoryId = SimdJsonInterop.GetString(json, "inventoryId")
            };

            int itemCount = SimdJsonInterop.get_array_length(json, "items");
            for (int i = 0; i < itemCount; i++)
            {
                string itemId = SimdJsonInterop.GetArrayString(json, "items", i, "itemId");
                int quantity = SimdJsonInterop.get_array_int(json, "items", i, "quantity");

                contextData.Items.Add(new ItemContextData
                {
                    ItemId = itemId,
                    Quantity = quantity
                });
            }

            return contextData;
        }
        catch (Exception e)
        {
            SBGDebug.LogError($"Error parsing inventory JSON: {e.Message}", "InventoryJsonParser | ParseInventoryContext");
            SBGDebug.LogError($"Exception StackTrace: {e.StackTrace}", "InventoryJsonParser | ParseInventoryContext");
            return null;
        }
    }
}