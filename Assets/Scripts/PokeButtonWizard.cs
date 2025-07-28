// Copyright (c) Meta Platforms, Inc. and affiliates.
// All rights reserved.
//
// This source code is licensed under the license found in the
// LICENSE file in the root directory of this source tree.

using UnityEngine;
using Oculus.Interaction;
using Oculus.Interaction.Surfaces;
using System.Reflection;

/// <summary>
/// This is a utility script to automatically configure a GameObject to be a pokeable button.
/// It adds and correctly links all the necessary components from the Meta Interaction SDK,
/// eliminating the need for complex manual setup in the Inspector.
///
/// To use:
/// 1. Add this component to the GameObject that has your button's collider.
/// 2. Click the three dots on the component in the Inspector and select "Auto-setup Poke Button".
/// 3. The script will add and configure everything. You can then remove this wizard script.
/// </summary>
[DisallowMultipleComponent]
public class PokeButtonWizard : MonoBehaviour
{
    [ContextMenu("Auto-setup Poke Button")]
    private void AutoSetupPokeButton()
    {
        // --- Step 1: Ensure a Collider exists ---
        if (!TryGetComponent<Collider>(out Collider col))
        {
            Debug.LogError($"[PokeButtonWizard] No Collider found on '{gameObject.name}'. Please add a Collider (like BoxCollider) first.", this);
            return;
        }
        if (col.isTrigger)
        {
            Debug.LogWarning($"[PokeButtonWizard] The Collider on '{gameObject.name}' is set to 'Is Trigger'. For a PokeInteractable, it should not be a trigger. Unchecking it for you.", this);
            col.isTrigger = false;
        }

        // --- Step 2: Add and Configure ClippedPlaneSurface ---
        if (!TryGetComponent<ClippedPlaneSurface>(out ClippedPlaneSurface surface))
        {
            surface = gameObject.AddComponent<ClippedPlaneSurface>();
            Debug.Log($"[PokeButtonWizard] Added ClippedPlaneSurface to '{gameObject.name}'.", this);
        }
        
        // Use reflection to set the private _plane field, as there is no public Inject method.
        FieldInfo planeField = typeof(ClippedPlaneSurface).GetField("_plane", BindingFlags.NonPublic | BindingFlags.Instance);
        if (planeField != null)
        {
            planeField.SetValue(surface, this.transform);
            Debug.Log($"[PokeButtonWizard] Linked Transform to ClippedPlaneSurface using reflection.", this);
        }
        else
        {
            Debug.LogError($"[PokeButtonWizard] Could not find the private field '_plane' on ClippedPlaneSurface. The SDK might have changed.", this);
            return;
        }


        // --- Step 3: Add and Configure PokeInteractable ---
        if (!TryGetComponent<PokeInteractable>(out PokeInteractable pokeInteractable))
        {
            pokeInteractable = gameObject.AddComponent<PokeInteractable>();
            Debug.Log($"[PokeButtonWizard] Added PokeInteractable to '{gameObject.name}'.", this);
        }
        // Programmatically assign the surface to the 'SurfacePatch' field.
        pokeInteractable.InjectSurfacePatch(surface);
        Debug.Log($"[PokeButtonWizard] Linked ClippedPlaneSurface to PokeInteractable.", this);


        // --- Step 4: Add and Configure PointableUnityEventWrapper ---
        if (!TryGetComponent<PointableUnityEventWrapper>(out PointableUnityEventWrapper eventWrapper))
        {
            eventWrapper = gameObject.AddComponent<PointableUnityEventWrapper>();
            Debug.Log($"[PokeButtonWizard] Added PointableUnityEventWrapper to '{gameObject.name}'.", this);
        }
        // Programmatically assign the interactable to the 'Pointable' field.
        eventWrapper.InjectPointable(pokeInteractable);
        Debug.Log($"[PokeButtonWizard] Linked PokeInteractable to PointableUnityEventWrapper.", this);

        Debug.Log($"<color=green><b>[PokeButtonWizard] Auto-setup complete for '{gameObject.name}'.</b></color> You can now configure the events on the PointableUnityEventWrapper and then remove this wizard component.", this);
    }
}
