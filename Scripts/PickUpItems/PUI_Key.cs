using UnityEngine;

public class PUI_Key: PickUpItem
{
    [Header("Key Properties")]
    [Space(10)]

    [SerializeField] private int keyID = 0000;
    [SerializeField] private KeyType keyType = KeyType.Key;
}
