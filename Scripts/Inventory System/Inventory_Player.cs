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

public class Inv_Player : InventoryBase
{
    public Inv_Player(string inventoryId, string name, int capacity) : base(inventoryId, name, capacity, InventoryType.Player)
    {
    }
}