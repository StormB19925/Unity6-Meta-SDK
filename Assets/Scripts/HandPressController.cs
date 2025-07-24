// Import necessary namespaces for Unity, Meta Interaction SDK, and the BigBlit ActivePack.
using UnityEngine;
using Oculus.Interaction;
using BigBlit.ActivePack;
using UnityEngine.Assertions;

/// <summary>
/// This controller script acts as a bridge between the Meta Interaction SDK and the BigBlit ActivePack.
/// It listens for hand tracking select/unselect events and translates them into press/release actions
/// on an object that implements the IPressable interface.
/// </summary>
public class HandPressController : MonoBehaviour
{
    [Tooltip("The IPressable target that will receive the press and release events. This is typically the button component from the BigBlit asset.")]
    [SerializeField]
    private MonoBehaviour _pressableTarget;

    private IPressable _pressable;

    private void Awake()
    {
        // Find the IPressable interface on the assigned target.
        if (_pressableTarget != null)
        {
            _pressable = _pressableTarget.GetComponent<IPressable>();
        }
        else
        {
            // If no target is assigned, try to find it on the same GameObject.
            _pressable = GetComponent<IPressable>();
        }

        // Ensure that a valid IPressable target was found.
        Assert.IsNotNull(_pressable, $"The {nameof(HandPressController)} requires a component that implements IPressable.");
    }

    /// <summary>
    /// Public method to be called by a UnityEvent when a hand selects this object.
    /// This triggers the Press() action on the target.
    /// </summary>
    public void HandleSelect()
    {
        if (_pressable != null)
        {
            _pressable.Press();
        }
    }

    /// <summary>
    /// Public method to be called by a UnityEvent when a hand unselects this object.
    /// This triggers the Normal() action (release) on the target.
    /// </summary>
    public void HandleUnselect()
    {
        if (_pressable != null)
        {
            _pressable.Normal();
        }
    }
}
