using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] private GameObject playerPhysicsPrefab;
    [SerializeField] private GameObject playerVisualPrefab;
    [SerializeField] private Transform spawnPoint;

    private void Start()
    {
        SpawnPlayer();
    }

    private void SpawnPlayer()
    {
        //Instantiate the player visual prefab (meshes, animations, camera, and scripts)
        GameObject playerVisual = Instantiate(playerVisualPrefab, spawnPoint.position, spawnPoint.rotation);
        //Instantiate the player physics prefab (colliders, rigidbodies, and scripts)
        GameObject playerPhysics = Instantiate(playerPhysicsPrefab, spawnPoint.position, spawnPoint.rotation);
    }

}
