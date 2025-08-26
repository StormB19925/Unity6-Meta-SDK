using UnityEngine;
using Oculus.Interaction;
using Oculus.Interaction.Input;

/// <summary>
/// This script manages a UI menu that is attached to the user's hand.
/// It shows the menu when the palm is facing upwards (towards the user's face)
/// and hides it otherwise. This provides an intuitive way to access a menu in VR.
///
/// To use this script:
/// 1. Create a new GameObject in your scene to act as the controller (e.g., "HandMenuController").
/// 2. Attach this script to the GameObject.
/// 3. Create your UI menu as a world-space Canvas and assign it to the `_handMenu` field.
/// 4. In your scene, locate your OVRCameraRig or equivalent XR setup. Find the `OVRHand` component
///    for the left hand and assign it to the `_hand` field.
/// 5. The script will automatically handle showing, hiding, and positioning the menu.
/// </summary>
public class HandMenuController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The UI GameObject to be shown or hidden. This should be a world-space canvas.")]
    [SerializeField]
    private GameObject _handMenu;

    [Tooltip("The hand to track for the menu gesture. Assign the left hand's OVRHand component here.")]
    [SerializeField]
    private OVRHand _hand;

    [Header("Settings")]
    [Tooltip("The angle in degrees from the 'up' direction to trigger the menu's visibility. A lower value means the palm must be more directly facing up.")]
    [SerializeField]
    private float _showAngleThreshold = 45.0f;

    [Tooltip("The angle in degrees from the 'up' direction to trigger hiding the menu. This should be larger than the show threshold to prevent flickering.")]
    [SerializeField]
    private float _hideAngleThreshold = 65.0f;

    [Tooltip("The offset from the hand's position to place the menu.")]
    [SerializeField]
    private Vector3 _positionOffset = new Vector3(0, 0.1f, 0.1f);

    [Tooltip("The speed at which the menu follows the hand's position and rotation.")]
    [SerializeField]
    private float _followSpeed = 8.0f;

    private bool _isMenuVisible = false;

    void Start()
    {
        if (_handMenu == null)
        {
            Debug.LogError("[HandMenuController] Hand Menu reference is not set in the inspector.", this);
            enabled = false;
            return;
        }

        if (_hand == null)
        {
            Debug.LogError("[HandMenuController] Hand reference is not set. Please assign the OVRHand component.", this);
            enabled = false;
            return;
        }

        // Start with the menu hidden
        _handMenu.SetActive(false);
        _isMenuVisible = false;
    }

    void Update()
    {
        if (!_hand.IsTracked)
        {
            // If the hand is not being tracked, ensure the menu is hidden
            if (_isMenuVisible)
            {
                _handMenu.SetActive(false);
                _isMenuVisible = false;
            }
            return;
        }

        // Get the direction the palm is facing (the hand's up vector)
        Vector3 palmUpDirection = _hand.transform.up;

        // Get the camera's up direction to compare against
        Vector3 referenceUpDirection = Camera.main.transform.up;

        // Calculate the angle between the palm's up direction and the camera's up direction
        float angle = Vector3.Angle(palmUpDirection, referenceUpDirection);

        // Determine whether to show or hide the menu based on the angle
        if (angle < _showAngleThreshold)
        {
            if (!_isMenuVisible)
            {
                _handMenu.SetActive(true);
                _isMenuVisible = true;
            }
        }
        else if (angle > _hideAngleThreshold)
        {
            if (_isMenuVisible)
            {
                _handMenu.SetActive(false);
                _isMenuVisible = false;
            }
        }

        // If the menu is visible, update its position and rotation to follow the hand
        if (_isMenuVisible)
        {
            UpdateMenuTransform();
        }
    }

    /// <summary>
    /// Smoothly updates the menu's position and rotation to follow the hand.
    /// </summary>
    private void UpdateMenuTransform()
    {
        // Calculate the target position with the offset relative to the hand's rotation
        Vector3 targetPosition = _hand.transform.position + (_hand.transform.rotation * _positionOffset);

        // The menu should look away from the palm
        Quaternion targetRotation = _hand.transform.rotation * Quaternion.LookRotation(Vector3.up, Vector3.forward);

        // Smoothly move the menu to the target position and rotation
        _handMenu.transform.position = Vector3.Lerp(_handMenu.transform.position, targetPosition, Time.deltaTime * _followSpeed);
        _handMenu.transform.rotation = Quaternion.Slerp(_handMenu.transform.rotation, targetRotation, Time.deltaTime * _followSpeed);
    }
}