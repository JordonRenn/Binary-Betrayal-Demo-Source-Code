/* 
INHERITANCE STRUCTURE:
IInventory
├── InventoryBase (abstract class)
│   ├── Inv_Container
│   ├── Inv_NPC
│   ├── Inv_Container
│   └── Inv_Player
InventoryData (struct)
IInventoryExchange 
 */

public class Inv_Container : InventoryBase
{
    public Inv_Container(string inventoryId, string name, int capacity) : base(inventoryId, name, capacity, InventoryType.Container)
    {
    }
}