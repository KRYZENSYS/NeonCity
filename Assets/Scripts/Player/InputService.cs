// =============================================================================
// NeonCity — Services / InputService.cs
// -----------------------------------------------------------------------------
// Thin wrapper over the new Input System. Exposes gameplay-friendly methods
// (e.g. MoveAxis, JumpPressed) so PlayerController doesn't need to know about
// the InputAction asset directly. Makes rebinding and controller swap trivial.
//
// The InputActionAsset is created via Assets > Create > Input Actions in Unity.
// Hook all generated actions here.
// =============================================================================

using UnityEngine;
using UnityEngine.InputSystem;
using NeonCity.Core;

namespace NeonCity.Services
{
    public class InputService : NeonCity.Core.IService
    {
        private PlayerInput _playerInput;

        public void Initialize()
        {
            // The PlayerInput component is on the player prefab and is loaded by
            // PlayerBootstrap; we receive it on Awake via Bind(...).
        }

        public void Shutdown() { /* nothing */ }

        public void Bind(PlayerInput pi) => _playerInput = pi;

        // ---- Movement ------------------------------------------------------
        public Vector2 MoveAxis()  => ReadAxis("Move");
        public Vector2 LookAxis()  => ReadAxis("Look");

        // ---- Actions -------------------------------------------------------
        public bool JumpPressed()      => ReadButtonDown("Jump");
        public bool SprintHeld()       => ReadButton("Sprint");
        public bool CrouchHeld()       => ReadButton("Crouch");
        public bool AimHeld()          => ReadButton("Aim");
        public bool CameraSwitchPressed() => ReadButtonDown("CameraSwitch");
        public bool FirePressed()      => ReadButton("Fire");
        public bool ReloadPressed()    => ReadButtonDown("Reload");
        public bool PausePressed()     => ReadButtonDown("Pause");
        public bool QuitPressed()      => Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
        public bool InteractPressed()  => ReadButtonDown("Interact");

        // ---- Helpers -------------------------------------------------------
        private Vector2 ReadAxis(string action)
        {
            if (_playerInput == null) return Vector2.zero;
            return _playerInput.actions[action]?.ReadValue<Vector2>() ?? Vector2.zero;
        }

        private bool ReadButton(string action)
        {
            if (_playerInput == null) return false;
            return _playerInput.actions[action]?.IsPressed() ?? false;
        }

        private bool ReadButtonDown(string action)
        {
            if (_playerInput == null) return false;
            return _playerInput.actions[action]?.WasPressedThisFrame() ?? false;
        }
    }
}