using UnityEngine;

public abstract class ContainerBase : SauceObject
{
    [Header("Container Settings")]
    [Space(10)]
    
    [SerializeField] private string containerInventoryID; //will be used to load inventory from json

    private const string FILE_ITEM_MASTER = "streamingassets/ItemMaster.csv"; // maybe used to generate random items?
    private IInventory inventory;

    public override void Interact()
    {
        // Interaction logic goes here
    }

    public void SetInventory(IInventory newInventory)
    {
        inventory = newInventory;
    }
}