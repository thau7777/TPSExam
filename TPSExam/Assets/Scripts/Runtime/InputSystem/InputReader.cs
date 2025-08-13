using System;
using UnityEngine;
using UnityEngine.InputSystem;


[CreateAssetMenu(fileName = "InputReader", menuName = "ScriptableObjects/InputReader")]
public class InputReader : ScriptableObject, InputActions.IPlayerActions
{
    private InputActions input;

    public Action<Vector2> onMove;
    public Action<bool> onAim;
    public Action<bool> onShoot;
    public Action onChangeShootingMethod;
    public Action onJump;
    private void OnEnable()
    {
        if (input == null)
        {
            input = new InputActions();
            input.Player.SetCallbacks(this);
        }
        input.Player.Enable();
    }

    private void OnDisable()
    {
        if (input != null)
        {
            input.Player.Disable();
        }

    }

    public void OnMove(InputAction.CallbackContext context)
    {
        onMove?.Invoke(context.ReadValue<Vector2>());
    }

    public void OnAim(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            onAim?.Invoke(true);
        }
        else if (context.canceled)
        {
            onAim?.Invoke(false);
        }
    }

    public void OnShoot(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            onShoot?.Invoke(true);
        }
        else if (context.canceled)
        {
            onShoot?.Invoke(false);
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            onJump?.Invoke();
        }
    }

    public void OnChangeShootingMethod(InputAction.CallbackContext context)
    {
        if (context.performed)
            onChangeShootingMethod?.Invoke();
    }

}
