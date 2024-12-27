using UnityEngine;

/// <summary>
/// Handles the spawning of the player character in the game.
/// </summary>
public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] private GameObject playerPhysicsPrefab;
    [SerializeField] private GameObject playerVisualPrefab;
    [SerializeField] private Transform spawnPoint;

    /// <summary>
    /// Called on the frame when a script is enabled just before any of the Update methods are called the first time.
    /// </summary>
    private void Start()
    {
        SpawnPlayer();
    }

    /// <summary>
    /// Instantiates the player visual and physics prefabs at the spawn point.
    /// </summary>
    private void SpawnPlayer()
    {
        //Instantiate the player visual prefab (meshes, animations, camera, and scripts)
        GameObject playerVisual = Instantiate(playerVisualPrefab, spawnPoint.position, spawnPoint.rotation);
        //Instantiate the player physics prefab (colliders, rigidbodies, and scripts)
        GameObject playerPhysics = Instantiate(playerPhysicsPrefab, spawnPoint.position, spawnPoint.rotation);
    }
}
