using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

/// <summary>
/// This script manages the spawning of specified prefabs at a designated spawn point.
/// It is designed to be controlled by UI buttons or other game events.
/// </summary>
public class AssetSpawner : MonoBehaviour
{
    [Tooltip("A list of all the prefabs that can be spawned by this spawner.")]
    [SerializeField]
    private List<GameObject> _spawnablePrefabs;

    [Tooltip("The transform representing the position and rotation where the assets will be spawned.")]
    [SerializeField]
    private Transform _spawnPoint;

    private void Awake()
    {
        // Ensure that the spawn point has been assigned in the Inspector.
        Assert.IsNotNull(_spawnPoint, $"The {nameof(AssetSpawner)} requires a {nameof(_spawnPoint)} to be assigned in the Inspector.");
    }

    /// <summary>
    /// Spawns a specified prefab at the spawn point. This function is intended to be called
    /// from a UnityEvent, such as a UI Button's OnClick event.
    /// </summary>
    /// <param name="prefabToSpawn">The GameObject prefab to instantiate.</param>
    public void SpawnAsset(GameObject prefabToSpawn)
    {
        // Check if the provided prefab is valid and part of the spawnable list.
        if (prefabToSpawn == null)
        {
            Debug.LogError("[AssetSpawner] The prefab to spawn is null. Please assign a prefab to the event trigger.", this);
            return;
        }

        if (!_spawnablePrefabs.Contains(prefabToSpawn))
        {
            Debug.LogWarning($"[AssetSpawner] The prefab '{prefabToSpawn.name}' is not in the list of spawnable prefabs. Spawning it anyway, but you might want to add it to the list.", this);
        }

        // Instantiate the prefab at the spawn point's position and rotation.
        GameObject spawnedObject = Instantiate(prefabToSpawn, _spawnPoint.position, _spawnPoint.rotation);
        // Set the scale of the spawned object to match the spawn point's scale.
        spawnedObject.transform.localScale = _spawnPoint.localScale;

        Debug.Log($"[AssetSpawner] Spawned '{spawnedObject.name}' at '{_spawnPoint.name}'.", this);
    }
}
