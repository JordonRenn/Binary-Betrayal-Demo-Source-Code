using System.Collections;
using UnityEngine;

/// <summary>
/// Handles the spawning of the player character in the game.
/// singleton? but destroys existing instances 
/// </summary>
public class PlayerSpawner : MonoBehaviour
{
    public static PlayerSpawner Instance {get; private set;}
    
    [SerializeField] private GameObject player;
    [SerializeField] private Transform spawnPoint;
    float spawnDelay = 0.1f;

    void Awake()
    {
        Debug.Log("PLAYER SPAWNER | Instantiated");
        
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(Instance);
            Instance = this;
        }
    }

    /// <summary>
    /// Instantiates the player visual and physics prefabs at the spawn point.
    /// </summary>
    public void SpawnPlayer()
    {
        Debug.Log("PLAYER SPAWNER | Spawning player");
        Instantiate(player, spawnPoint.position, spawnPoint.rotation);
    }
}
