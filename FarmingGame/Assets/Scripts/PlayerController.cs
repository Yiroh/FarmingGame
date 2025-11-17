using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance;

    private void Awake()
    {
        // Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    [Header("Movement")]
    public float moveSpeed = 5f;

    [Header("Grid Interaction")]
    public GridSystem gridSystem;
    public GridAction currentAction = GridAction.None; // Start with no tool selected

    private void Update()
    {
        HandleMovement();
        HandleInteractionInput();
        HandleActionSwitching();
        HandleMouseInteraction();
    }

    #region Movement

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

    #endregion

    #region Keyboard Interaction

    private void HandleInteractionInput()
    {
        if (gridSystem == null || Keyboard.current == null) return;

        // Space bar triggers interaction at highlighted cell
        if (Keyboard.current.spaceKey.wasPressedThisFrame && currentAction != GridAction.None)
        {
            gridSystem.TryInteractWithHighlighted(currentAction);
        }
    }

    private void HandleActionSwitching()
    {
        if (Keyboard.current == null) return;

        // Dev / debug hotkeys for manually choosing tools
        if (Keyboard.current.digit1Key.wasPressedThisFrame) currentAction = GridAction.Place;
        if (Keyboard.current.digit2Key.wasPressedThisFrame) currentAction = GridAction.Delete;
        if (Keyboard.current.digit3Key.wasPressedThisFrame) currentAction = GridAction.Select;
        if (Keyboard.current.digit4Key.wasPressedThisFrame) currentAction = GridAction.Harvest;

        // B = toggle build mode (future: open radial building UI here)
        if (Keyboard.current.bKey.wasPressedThisFrame)
        {
            if (currentAction == GridAction.Place)
            {
                currentAction = GridAction.None;   // exit build mode
            }
            else
            {
                currentAction = GridAction.Place;  // enter build mode with current prefab

                // Later:
                // - Show your circular / radial building UI
                // - Let player choose a building type
                // - Call gridSystem.SetPlacePrefab(selectedPrefab);
            }
        }
    }

    #endregion

    #region Mouse Interaction

    private void HandleMouseInteraction()
    {
        if (gridSystem == null || Mouse.current == null) return;

        // Left mouse click interacts at the highlighted cell
        if (Mouse.current.leftButton.wasPressedThisFrame && currentAction != GridAction.None)
        {
            gridSystem.TryInteractWithHighlighted(currentAction);
        }
    }

    #endregion
}
