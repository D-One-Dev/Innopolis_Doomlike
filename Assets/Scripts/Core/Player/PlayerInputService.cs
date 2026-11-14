using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

public sealed class PlayerInputService : IInitializable, IDisposable
{
    private readonly InputSystem_Actions _actions = new InputSystem_Actions();
    private bool _initialized;

    public Vector2 MoveInput => _actions.Player.Move.ReadValue<Vector2>();
    public bool AttackHeld => _actions.Player.Attack.IsPressed();
    public bool SprintHeld => _actions.Player.Sprint.IsPressed();

    public event Action AttackPressed;
    public event Action JumpPressed;
    public event Action PreviousWeaponPressed;
    public event Action NextWeaponPressed;

    public void Initialize()
    {
        _actions.Player.Enable();
        _actions.Player.Attack.performed += OnAttackPerformed;
        _actions.Player.Jump.performed += OnJumpPerformed;
        _actions.Player.Previous.performed += OnPreviousPerformed;
        _actions.Player.Next.performed += OnNextPerformed;
        _initialized = true;
    }

    public void Dispose()
    {
        if (!_initialized)
        {
            return;
        }

        _actions.Player.Attack.performed -= OnAttackPerformed;
        _actions.Player.Jump.performed -= OnJumpPerformed;
        _actions.Player.Previous.performed -= OnPreviousPerformed;
        _actions.Player.Next.performed -= OnNextPerformed;
        _actions.Player.Disable();
        _actions.Dispose();
        _initialized = false;
    }

    public Vector2 ConsumeLookDelta()
    {
        return _actions.Player.Look.ReadValue<Vector2>();
    }

    private void OnAttackPerformed(InputAction.CallbackContext context)
    {
        AttackPressed?.Invoke();
    }

    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        JumpPressed?.Invoke();
    }

    private void OnPreviousPerformed(InputAction.CallbackContext context)
    {
        PreviousWeaponPressed?.Invoke();
    }

    private void OnNextPerformed(InputAction.CallbackContext context)
    {
        NextWeaponPressed?.Invoke();
    }
}
