using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

/// <summary>
/// This script manages the spawning of specified prefabs at a designated spawn point.
/// It is designed to be controlled by UI buttons or other game events, providing a centralized
/// and reusable system for object instantiation.
/// </summary>
public class AssetSpawner : MonoBehaviour
{
    [Header("Configuration")]
    [Tooltip("A list of all the prefabs that are allowed to be spawned by this system. This is used for validation.")]
    [SerializeField]
    private List<GameObject> _spawnablePrefabs;

    [Tooltip("The transform that defines the spawn location. The spawned object will match this transform's position, rotation, and scale.")]
    [SerializeField]
    private Transform _spawnPoint;

    /// <summary>
    /// Called when the script instance is being loaded.
    /// Validates that essential references have been assigned in the Inspector.
    /// </summary>
    private void Awake()
    {
        // Assert that a spawn point has been assigned. Without this, the script cannot function.
        // This provides a clear error in the console if the setup is incomplete.
        Assert.IsNotNull(_spawnPoint, $"[AssetSpawner] The '{nameof(_spawnPoint)}' field is not assigned on '{this.gameObject.name}'. Please assign a Transform to act as the spawn point.");
    }

    /// <summary>
    /// Spawns a specified prefab at the designated spawn point.
    /// This function is intended to be called from a UnityEvent, such as a UI Button's OnClick event.
    /// </summary>
    /// <param name="prefabToSpawn">The GameObject prefab to instantiate. This should be assigned from the UnityEvent in the Inspector.</param>
    public void SpawnAsset(GameObject prefabToSpawn)
    {
        // First, validate that the prefab passed from the event is not null.
        if (prefabToSpawn == null)
        {
            Debug.LogError("[AssetSpawner] The prefab to spawn is null. Please ensure the UnityEvent is correctly configured with a valid prefab.", this);
            return;
        }

        // For good practice, check if the requested prefab is in our pre-approved list.
        // This is not a strict requirement but can help catch configuration errors.
        if (!_spawnablePrefabs.Contains(prefabToSpawn))
        {
            Debug.LogWarning($"[AssetSpawner] The prefab '{prefabToSpawn.name}' is not in the list of spawnable prefabs. Spawning it anyway, but you might want to add it to the list for better tracking.", this);
        }

        // Instantiate the new object, giving it the spawn point's position and rotation.
        GameObject spawnedObject = Instantiate(prefabToSpawn, _spawnPoint.position, _spawnPoint.rotation);
        
        // Set the scale of the newly spawned object to match the spawn point's scale.
        // This allows for easy visual setup of the spawned object's size in the editor.
        spawnedObject.transform.localScale = _spawnPoint.localScale;

        Debug.Log($"[AssetSpawner] Successfully spawned '{spawnedObject.name}' at '{_spawnPoint.name}'.", this);
    }
}
