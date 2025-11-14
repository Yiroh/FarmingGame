using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;

    [Header("Interaction")]
    public Transform interactionBox;      // Assign a child cube here
    public float interactionDistance = 1f;
    public Vector3 interactionBoxSize = new Vector3(1f, 1f, 1f);
    public LayerMask interactionLayers;   // Optional: layers to interact with

    private void Start()
    {
        if (interactionBox != null)
        {
            interactionBox.localPosition = new Vector3(0f, 0f, interactionDistance);
            interactionBox.localScale = interactionBoxSize;

            var renderer = interactionBox.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                // translucent blue
                Color c = new Color(0f, 0.5f, 1f, 0.25f);
                renderer.material.color = c;
            }
        }
        else
        {
            Debug.LogWarning("PlayerController: No interactionBox assigned in the inspector.");
        }
    }

    private void Update()
    {
        HandleMovement();
        HandleInteractionBoxPosition();
        HandleInteractionInput();
    }

    private void HandleMovement()
    {
        if (Keyboard.current == null) return;

        float h = 0f;
        float v = 0f;

        // WASD using new Input System
        if (Keyboard.current.aKey.isPressed) h -= 1f;
        if (Keyboard.current.dKey.isPressed) h += 1f;
        if (Keyboard.current.sKey.isPressed) v -= 1f;
        if (Keyboard.current.wKey.isPressed) v += 1f;

        Vector3 moveDir = new Vector3(h, 0f, v).normalized;

        if (moveDir.sqrMagnitude > 0.001f)
        {
            transform.position += moveDir * moveSpeed * Time.deltaTime;
            transform.forward = moveDir;
        }
    }

    private void HandleInteractionBoxPosition()
    {
        if (interactionBox == null) return;

        interactionBox.position = transform.position + transform.forward * interactionDistance;
        interactionBox.localScale = interactionBoxSize;
    }

    private void HandleInteractionInput()
    {
        if (Keyboard.current == null) return;

        // Space bar using new Input System
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            Debug.Log("Interaction pressed!");

            if (interactionBox != null)
            {
                Vector3 center = interactionBox.position;
                Vector3 halfExtents = interactionBoxSize * 0.5f;

                Collider[] hits = Physics.OverlapBox(center, halfExtents, Quaternion.identity, interactionLayers);

                foreach (var hit in hits)
                {
                    Debug.Log("Hit interactable: " + hit.name);
                    // Later: hit.GetComponent<IInteractable>()?.Interact();
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (interactionBox == null) return;

        Gizmos.color = new Color(0f, 0.5f, 1f, 0.35f);
        Gizmos.DrawCube(interactionBox.position, interactionBoxSize);
    }
}
