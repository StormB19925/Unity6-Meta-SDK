using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

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

    /// <summary>
    /// A public accessor to get a read-only version of the spawnable prefabs list.
    /// This allows other scripts to see what can be spawned without allowing them to modify the list.
    /// </summary>
    public IReadOnlyList<GameObject> SpawnablePrefabs => _spawnablePrefabs;

    [Header("Spawning Surface")]
    [Tooltip("The collider of the surface where objects will be spawned (e.g., a table).")]
    [SerializeField]
    private Collider _spawnSurface;

    [Tooltip("The layers that can block spawning. This should include other spawned objects.")]
    [SerializeField]
    private LayerMask _blockingLayers;

    [Tooltip("The maximum number of attempts to find an empty spawn position before giving up.")]
    [SerializeField]
    private int _maxSpawnAttempts = 50;

    /// <summary>
    /// Called when the script instance is being loaded.
    /// Validates that essential references have been assigned in the Inspector.
    /// </summary>
    private void Awake()
    {
        // Assert that a spawn surface has been assigned.
        Assert.IsNotNull(_spawnSurface, $"[AssetSpawner] The '{nameof(_spawnSurface)}' field is not assigned on '{gameObject.name}'. Please assign a Collider to act as the spawn surface.");
    }

    /// <summary>
    /// Spawns a specified prefab at a valid position on the designated spawn surface.
    /// </summary>
    /// <param name="prefabToSpawn">The GameObject prefab to instantiate.</param>
    public void SpawnAsset(GameObject prefabToSpawn)
    {
        if (prefabToSpawn == null)
        {
            Debug.LogError("[AssetSpawner] The prefab to spawn is null. Please ensure the UnityEvent is correctly configured with a valid prefab.", this);
            return;
        }

        if (!_spawnablePrefabs.Contains(prefabToSpawn))
        {
            Debug.LogWarning($"[AssetSpawner] The prefab '{prefabToSpawn.name}' is not in the list of spawnable prefabs. Spawning it anyway, but you might want to add it to the list for better tracking.", this);
        }

        if (TryFindPositionOnSurface(prefabToSpawn, out Vector3 position, out Quaternion rotation))
        {
            GameObject spawnedObject = Instantiate(prefabToSpawn, position, rotation);
            Debug.Log($"[AssetSpawner] Successfully spawned '{spawnedObject.name}'.", this);
        }
        else
        {
            Debug.LogWarning($"[AssetSpawner] Could not find an empty position on the surface for '{prefabToSpawn.name}' after {_maxSpawnAttempts} attempts.", this);
        }
    }

    /// <summary>
    /// Attempts to find a random, unoccupied position on the spawn surface.
    /// </summary>
    /// <param name="prefabToSpawn">The prefab to be spawned, used to determine its size.</param>
    /// <param name="position">The found position on the surface.</param>
    /// <param name="rotation">The appropriate rotation for the object to sit flat on the surface.</param>
    /// <returns>True if a valid position was found, otherwise false.</returns>
    private bool TryFindPositionOnSurface(GameObject prefabToSpawn, out Vector3 position, out Quaternion rotation)
    {
        Bounds surfaceBounds = _spawnSurface.bounds;
        if (!prefabToSpawn.TryGetComponent<Collider>(out var prefabCollider))
        {
            Debug.LogError($"[AssetSpawner] Prefab '{prefabToSpawn.name}' needs a Collider to be spawned correctly.", this);
            position = Vector3.zero;
            rotation = Quaternion.identity;
            return false;
        }

        Vector3 prefabExtents = prefabCollider.bounds.extents;

        for (int i = 0; i < _maxSpawnAttempts; i++)
        {
            // Pick a random point within the horizontal bounds of the surface
            float randomX = Random.Range(surfaceBounds.min.x, surfaceBounds.max.x);
            float randomZ = Random.Range(surfaceBounds.min.z, surfaceBounds.max.z);
            Vector3 rayStart = new(randomX, surfaceBounds.max.y + 0.1f, randomZ);

            // Raycast down to find the exact point on the surface
            if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, surfaceBounds.size.y + 0.2f))
            {
                // Check if we hit the correct spawn surface
                if (hit.collider == _spawnSurface)
                {
                    Vector3 potentialPosition = hit.point + (hit.normal * prefabExtents.y);
                    Quaternion potentialRotation = Quaternion.FromToRotation(Vector3.up, hit.normal);

                    // Check if the space is occupied by other objects
                    if (!Physics.CheckBox(potentialPosition, prefabExtents, potentialRotation, _blockingLayers))
                    {
                        position = potentialPosition;
                        rotation = potentialRotation;
                        return true;
                    }
                }
            }
        }

        // Could not find a suitable position
        position = Vector3.zero;
        rotation = Quaternion.identity;
        return false;
    }
}

/// <summary>
/// This script acts as the controller for a UI Toolkit menu that displays spawnable assets.
/// It connects to an AssetSpawner, populates a ListView with the available prefabs,
/// and handles the click events to trigger the spawning of the selected asset.
/// </summary>
[RequireComponent(typeof(UIDocument))]
public class AssetMenuController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The AssetSpawner that holds the list of prefabs and the spawn logic.")]
    [SerializeField]
    private AssetSpawner _assetSpawner;

    // The name of the ListView element in the UXML file.
    private const string ItemListName = "Item_Selection_List";

    private ListView _itemListView;

    private void Awake()
    {
        // Ensure the AssetSpawner has been assigned in the Inspector.
        Assert.IsNotNull(_assetSpawner, $"The {nameof(AssetMenuController)} requires an {nameof(_assetSpawner)} to be assigned.");
    }

    private void OnEnable()
    {
        // Get the root of the UI Document.
        var root = GetComponent<UIDocument>().rootVisualElement;

        // Find the ListView by its name.
        _itemListView = root.Q<ListView>(ItemListName);

        if (_itemListView == null)
        {
            Debug.LogError($"[AssetMenuController] Could not find a ListView named '{ItemListName}' in the UI Document.", this);
            return;
        }

        // Populate the list with the prefabs from the spawner.
        PopulateList();
    }

    /// <summary>
    /// Sets up the ListView with the data from the AssetSpawner.
    /// </summary>
    private void PopulateList()
    {
        // Get the read-only list of prefabs from the spawner.
        var spawnableItems = _assetSpawner.SpawnablePrefabs;

        // The "makeItem" function is a factory that creates a new visual element for an item in the list.
        _itemListView.makeItem = () => new Button();

        // The "bindItem" function connects a data item to a visual element.
        _itemListView.bindItem = (visualElement, index) =>
        {
            // Cast the visual element to a Button.
            var button = visualElement as Button;
            
            // Get the specific prefab for this list item.
            GameObject prefab = spawnableItems[index];

            // Set the button's text to the name of the prefab.
            button.text = prefab.name;

            // Register a callback for when the button is clicked.
            button.clicked += () =>
            {
                // When clicked, call the spawner's function with the correct prefab.
                _assetSpawner.SpawnAsset(prefab);
                Debug.Log($"[AssetMenuController] Requested to spawn '{prefab.name}'.");
            };
        };

        // Set the data source for the ListView. The itemsSource property requires an IList,
        // so we convert the IReadOnlyList to a new List.
        _itemListView.itemsSource = new List<GameObject>(spawnableItems);
    }
}
