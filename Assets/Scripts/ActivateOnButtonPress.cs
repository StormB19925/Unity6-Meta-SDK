// Import necessary namespaces for Unity and Meta XR SDKs.
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// Conditional compilation for Meta Core SDK.
#if META_CORE
using Meta.XR.Core;
#endif

// Conditional compilation for Meta Interaction SDK, specifically for Building Blocks and Interaction components.
#if META_INTERACTION
using Meta.XR.BuildingBlocks;
using Meta.XR.Interaction;
#endif

// Conditional compilation for Meta MR Utility Kit.
#if META_MR_UTILITY_KIT
using Meta.XR.MRUtilityKit;
#endif

/// <summary>
/// This script activates a list of GameObjects when a button, interacted with via hand tracking, is pressed.
/// It requires the Meta Interaction SDK and a ButtonController component on the same GameObject.
/// </summary>
public class ActivateOnButtonPress : MonoBehaviour
{
    [Tooltip("Assign the GameObjects to activate when the button is pressed.")]
    public List<GameObject> objectsToActivate;

    [Tooltip("This event is triggered when the button is pressed by a hand.")]
    public UnityEvent onButtonPressed;

#if META_INTERACTION
    // Private reference to the ButtonController component from Meta's Building Blocks.
    private ButtonController _buttonController;
    // Private reference to the PokeInteractor, used to ensure hand tracking is active.
    private PokeInteractor _pokeInteractor;
#endif

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// It initializes the button and hand tracking components.
    /// </summary>
    void Awake()
    {
        // This block only compiles if the Meta Interaction SDK is installed.
#if META_INTERACTION
        // Get the ButtonController component attached to this GameObject.
        _buttonController = GetComponent<ButtonController>();
        if (_buttonController == null)
        {
            // Log an error if the ButtonController is missing, as it's essential for functionality.
            Debug.LogError("ButtonController component not found on this GameObject. Please add a ButtonController component.", this);
            return;
        }

        // Find the PokeInteractor in the scene to confirm hand tracking is set up.
        _pokeInteractor = FindObjectOfType<PokeInteractor>();
        if (_pokeInteractor == null)
        {
            // Log an error if the PokeInteractor is not found, as it's required for hand interaction.
            Debug.LogError("PokeInteractor not found in the scene. This component is required for hand tracking interaction.", this);
            return;
        }

        // Subscribe the HandleButtonPressed method to the button's WhenSelected event.
        _buttonController.WhenSelected.AddListener(HandleButtonPressed);
#else
        // If the Meta Interaction SDK is not installed, log a warning.
        Debug.LogWarning("Meta Interaction SDK not found. Button interaction will not work.");
#endif
    }

    /// <summary>
    /// OnDestroy is called when the MonoBehaviour will be destroyed.
    /// It cleans up the event listener to prevent memory leaks.
    /// </summary>
    private void OnDestroy()
    {
        // This block only compiles if the Meta Interaction SDK is installed.
#if META_INTERACTION
        // Unsubscribe the HandleButtonPressed method from the button's WhenSelected event.
        if (_buttonController != null)
        {
            _buttonController.WhenSelected.RemoveListener(HandleButtonPressed);
        }
#endif
    }

    /// <summary>
    /// This method is called when the button's WhenSelected event is triggered.
    /// It activates the specified GameObjects and invokes the onButtonPressed event.
    /// </summary>
    private void HandleButtonPressed()
    {
        // Check if the list of objects to activate is not null.
        if (objectsToActivate != null)
        {
            // Iterate through the list of GameObjects.
            foreach (var obj in objectsToActivate)
            {
                // Check if the GameObject is not null and then set it to active.
                if (obj != null)
                {
                    obj.SetActive(true);
                }
            }
        }
        // Invoke the UnityEvent, triggering any methods assigned in the Inspector.
        onButtonPressed.Invoke();
    }
}
