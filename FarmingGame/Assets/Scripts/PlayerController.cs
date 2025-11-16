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
    public GridAction currentAction = GridAction.Place;

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
        // Space bar triggers interaction at highlighted cell
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            gridSystem.TryInteractWithHighlighted(currentAction);
        }
    }

    private void HandleActionSwitching()
    {
        if (Keyboard.current.digit1Key.wasPressedThisFrame) currentAction = GridAction.Place;
        if (Keyboard.current.digit2Key.wasPressedThisFrame) currentAction = GridAction.Delete;
        if (Keyboard.current.digit3Key.wasPressedThisFrame) currentAction = GridAction.Select;
        if (Keyboard.current.digit4Key.wasPressedThisFrame) currentAction = GridAction.Harvest;
    }

    #endregion

    #region Mouse Interaction

    private void HandleMouseInteraction()
    {
        if (Mouse.current == null) return;

        // Left mouse click interacts at the highlighted cell
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            gridSystem.TryInteractWithHighlighted(currentAction);
        }
    }

    #endregion
}
