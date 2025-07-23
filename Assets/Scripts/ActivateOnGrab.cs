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
using Oculus.Interaction;
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
    [Tooltip("A list of GameObjects that will be activated when the grabbable object is selected.")]
    [SerializeField]
    private List<GameObject> _objectsToActivate;

    [Tooltip("A UnityEvent that is invoked when the grabbable object is selected.")]
    [SerializeField]
    private UnityEvent _onGrabbed;

    [Tooltip("The GameObject containing the primary grab interactor (e.g., TouchHandGrabInteractable). This will be activated by the script.")]
    [SerializeField]
    private GameObject _handInteractor;

#if META_INTERACTION
    // A private reference to the Grabbable component from the Meta Interaction SDK.
    // This component manages the grabbable behavior of the object.
    private Grabbable _grabbable;

    // A private reference to the TouchHandGrabInteractable component.
    // This component enables hand-tracking-based grab interactions.
    private TouchHandGrabInteractable _touchHandGrabInteractable;

    /// <summary>
    /// The Awake method is called when the script instance is being loaded.
    /// It is used to initialize components and subscribe to events.
    /// </summary>
    private void Awake()
    {
        // If a hand interactor has been assigned, ensure it is active.
        // This is crucial for the grab functionality to work if the interactor starts as disabled.
        if (_handInteractor != null)
        {
            _handInteractor.SetActive(true);
        }

        // Log a message to indicate that the Awake method has been called.
        Debug.Log("ActivateOnGrab: Awake() called.", this);

        // Attempt to get the Grabbable component attached to this GameObject.
        _grabbable = GetComponent<Grabbable>();
        if (_grabbable == null)
        {
            // If the Grabbable component is not found, log an error and exit the method.
            // This component is essential for the script's functionality.
            Debug.LogError("Grabbable component not found on this GameObject. Please add a Grabbable component.", this);
            return;
        }
        // Log a message to confirm that the Grabbable component was found.
        Debug.Log("ActivateOnGrab: Found Grabbable component.", this);


        // Attempt to get the TouchHandGrabInteractable component in this GameObject or its children.
        _touchHandGrabInteractable = GetComponentInChildren<TouchHandGrabInteractable>();
        if (_touchHandGrabInteractable == null)
        {
            // If the TouchHandGrabInteractable component is not found, log an error and exit the method.
            // This component is required for hand-tracking grab interactions.
            Debug.LogError("TouchHandGrabInteractable component not found on this GameObject or its children. Please add a TouchHandGrabInteractable component.", this);
            return;
        }
        // Log a message to confirm that the TouchHandGrabInteractable component was found.
        Debug.Log("ActivateOnGrab: Found TouchHandGrabInteractable component.", this);

        // Subscribe the HandlePointerEvent method to the WhenPointerEventRaised event of the Grabbable component.
        // This allows the script to respond to pointer events, such as grabbing.
        _grabbable.WhenPointerEventRaised += HandlePointerEvent;
        Debug.Log("ActivateOnGrab: Subscribed to WhenPointerEventRaised event.", this);
    }

    /// <summary>
    /// The OnDestroy method is called when the MonoBehaviour will be destroyed.
    /// It is used to clean up resources, such as unsubscribing from events, to prevent memory leaks.
    /// </summary>
    private void OnDestroy()
    {
        // Check if the Grabbable component reference is not null before unsubscribing.
        if (_grabbable != null)
        {
            // Unsubscribe the HandlePointerEvent method from the WhenPointerEventRaised event.
            // This is important to prevent errors if the event is raised after this object has been destroyed.
            _grabbable.WhenPointerEventRaised -= HandlePointerEvent;
        }
    }

    /// <summary>
    /// This method is the event handler for the Grabbable's WhenPointerEventRaised event.
    /// It checks for the 'Select' event type and, if detected, activates a list of GameObjects.
    /// </summary>
    /// <param name="evt">The PointerEvent data associated with the event.</param>
    private void HandlePointerEvent(PointerEvent evt)
    {
        // Check if the event type is 'Select', which corresponds to a grab action.
        // If it's not a 'Select' event, the method returns and does nothing.
        if (evt.Type != PointerEventType.Select)
        {
            return;
        }

        // Log that a grab event has been handled, including the identifier of the interactor.
        Debug.Log("ActivateOnGrab: HandleGrabbed() called by " + evt.Identifier, this);

        // Check if the list of objects to activate has been assigned and is not empty.
        if (_objectsToActivate != null && _objectsToActivate.Count > 0)
        {
            // Log the number of objects that are about to be activated.
            Debug.Log($"ActivateOnGrab: Found {_objectsToActivate.Count} objects to activate.", this);
            // Iterate through each GameObject in the list.
            foreach (var obj in _objectsToActivate)
            {
                // Check if the GameObject reference is not null.
                if (obj != null)
                {
                    // Activate the GameObject.
                    Debug.Log($"ActivateOnGrab: Activating '{obj.name}'.", this);
                    obj.SetActive(true);
                }
                else
                {
                    // Log a warning if a null reference is found in the list.
                    Debug.LogWarning("ActivateOnGrab: An object in the 'objectsToActivate' list is null.", this);
                }
            }
        }
        else
        {
            // Log a warning if the list of objects to activate is null or empty.
            Debug.LogWarning("ActivateOnGrab: The 'objectsToActivate' list is null or empty. Please assign GameObjects in the Inspector.", this);
        }
        // Invoke the onGrabbed UnityEvent, which can be configured in the Inspector to call other methods.
        _onGrabbed.Invoke();
    }
#else
    /// <summary>
    /// The Awake method is called when the script instance is being loaded.
    /// In this case, it serves to inform the user that a required dependency is missing.
    /// </summary>
    private void Awake()
    {
        // If the Meta Interaction SDK is not available, log a warning to the console.
        // This indicates that the grab interaction functionality of this script will not work.
        Debug.LogWarning("Meta Interaction SDK not found. Grab interaction will not work.");
    }
#endif
}
