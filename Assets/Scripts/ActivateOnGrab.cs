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
/// This script activates a list of GameObjects when a grabbable object is interacted with via hand tracking.
/// It requires the Meta Interaction SDK and a Grabbable component on the same GameObject.
/// </summary>
public class ActivateOnGrab : MonoBehaviour
{
    [Tooltip("Assign the GameObjects to activate when the object is grabbed.")]
    public List<GameObject> objectsToActivate;

    [Tooltip("This event is triggered when the object is grabbed by a hand.")]
    public UnityEvent onGrabbed;

#if META_INTERACTION
    // Private reference to the Grabbable component from Meta's Interaction SDK.
    private Grabbable _grabbable;
    // Private reference to the TouchHandGrabInteractable component.
    private TouchHandGrabInteractable _touchHandGrabInteractable;

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// It initializes the grabbable component and subscribes to the grab event.
    /// </summary>
    void Awake()
    {
        // Get the Grabbable component attached to this GameObject.
        _grabbable = GetComponent<Grabbable>();
        if (_grabbable == null)
        {
            // Log an error if the Grabbable is missing, as it's essential for functionality.
            Debug.LogError("Grabbable component not found on this GameObject. Please add a Grabbable component.", this);
            return;
        }

        // Get the TouchHandGrabInteractable component in this GameObject or its children.
        _touchHandGrabInteractable = GetComponentInChildren<TouchHandGrabInteractable>();
        if (_touchHandGrabInteractable == null)
        {
            // Log an error if the TouchHandGrabInteractable is missing.
            Debug.LogError("TouchHandGrabInteractable component not found on this GameObject or its children. Please add a TouchHandGrabInteractable component.", this);
            return;
        }

        // Subscribe the HandleGrabbed method to the grabbable's WhenGrabbed event.
        _grabbable.WhenGrabbed.AddListener(HandleGrabbed);
    }

    /// <summary>
    /// OnDestroy is called when the MonoBehaviour will be destroyed.
    /// It cleans up the event listener to prevent memory leaks.
    /// </summary>
    private void OnDestroy()
    {
        // Unsubscribe the HandleGrabbed method from the grabbable's WhenSelected event.
        if (_grabbable != null)
        {
            _grabbable.WhenGrabbed.RemoveListener(HandleGrabbed);
        }
    }

    /// <summary>
    /// This method is called when the grabbable's WhenGrabbed event is triggered.
    /// It activates the specified GameObjects and invokes the onGrabbed event.
    /// </summary>
    private void HandleGrabbed(Grabber grabber)
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
        onGrabbed.Invoke();
    }
#else
    void Awake()
    {
        // If the Meta Interaction SDK is not installed, log a warning.
        Debug.LogWarning("Meta Interaction SDK not found. Grab interaction will not work.");
    }
#endif
}
