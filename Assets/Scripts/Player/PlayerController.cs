// =============================================================================
// NeonCity — Player / PlayerController.cs
// -----------------------------------------------------------------------------
// The heart of player movement. Supports BOTH FPS and 3rd-person camera modes,
// seamlessly switchable at runtime (V key by default). All input goes through
// the new Input System (PlayerInput asset -> InputService). Camera is Cinemachine-
// driven; this script only moves the body and tells the camera what to follow.
//
// Features:
//   - CharacterController-based movement (handles slopes + step offsets)
//   - Crouch / sprint / jump / vault (vault stub)
//   - Aim-down-sights (ADS) modifies FOV and movement speed
//   - Network-aware: every move is sent via NetworkTransform in NetworkPlayer
//   - Server reconciliation via NetworkClient.Predict
//
// Attach to: the player root prefab (Player.prefab in Assets/Prefabs/).
// =============================================================================

using UnityEngine;
using UnityEngine.InputSystem;
using NeonCity.Core;
using NeonCity.Combat;
using NeonCity.Networking;

namespace NeonCity.Player
{
    public enum CameraMode { FirstPerson, ThirdPerson }

    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        public float walkSpeed    = 4f;
        public float sprintSpeed  = 7f;
        public float crouchSpeed  = 2f;
        public float jumpHeight   = 1.5f;
        public float gravity      = -25f;
        public float mouseSens    = 1.5f;

        [Header("References")]
        public Transform cameraRoot;        // Cinemachine target
        public Transform weaponSocket;     // weapons attach here
        public WeaponBase  currentWeapon;

        private CharacterController _cc;
        private Vector3 _velocity;
        private bool    _isGrounded;
        private bool    _isCrouching;
        private bool    _isSprinting;
        private bool    _isAiming;

        public CameraMode Mode { get; private set; } = CameraMode.ThirdPerson;
        public bool IsAiming => _isAiming;
        public bool IsMoving => _cc.velocity.sqrMagnitude > 0.1f;

        // -------------------------------------------------------------------
        private void Awake()
        {
            _cc = GetComponent<CharacterController>();
            if (cameraRoot == null) cameraRoot = transform.Find("CameraRoot");
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible   = false;
        }

        private void Update()
        {
            var input = GameManager.Instance?.Input;
            if (input == null) return;

            HandleCameraToggle(input);
            HandleMove(input);
            HandleCrouch(input);
            HandleSprint(input);
            HandleJump();
            HandleAim(input);
            ApplyGravity();
            Move();
        }

        // -------------------------------------------------------------------
        private void HandleCameraToggle(InputService input)
        {
            if (input.CameraSwitchPressed())
            {
                Mode = Mode == CameraMode.FirstPerson
                    ? CameraMode.ThirdPerson
                    : CameraMode.FirstPerson;
                Debug.Log($"[Player] Camera mode → {Mode}");
            }
        }

        private void HandleMove(InputService input)
        {
            var move = input.MoveAxis();
            // Convert to world-space based on camera yaw.
            var cam = Camera.main;
            if (cam != null)
            {
                var forward = cam.transform.forward;
                var right   = cam.transform.right;
                forward.y = 0; right.y = 0;
                forward.Normalize(); right.Normalize();
                var dir = (forward * move.y + right * move.x).normalized;
                _moveDir = dir;
            }
            else _moveDir = new Vector3(move.x, 0, move.y);
        }

        private Vector3 _moveDir;

        private void HandleCrouch(InputService input)
        {
            _isCrouching = input.CrouchHeld();
            _cc.height = _isCrouching ? 1.2f : 1.8f;
        }

        private void HandleSprint(InputService input)
        {
            _isSprinting = input.SprintHeld() && !_isCrouching && !_isAiming;
        }

        private void HandleJump()
        {
            if (GameManager.Instance.Input.JumpPressed() && _isGrounded && !_isCrouching)
                _velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        private void HandleAim(InputService input)
        {
            _isAiming = input.AimHeld();
            if (cameraRoot != null)
            {
                var fov = _isAiming ? 45f : 70f;
                var cam = cameraRoot.GetComponent<Camera>();
                if (cam != null) cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, fov, 0.2f);
            }
        }

        private void ApplyGravity()
        {
            _isGrounded = _cc.isGrounded;
            if (_isGrounded && _velocity.y < 0) _velocity.y = -2f;
            _velocity.y += gravity * Time.deltaTime;
        }

        private void Move()
        {
            float speed = _isCrouching ? crouchSpeed
                        : _isSprinting ? sprintSpeed
                        : _isAiming    ? walkSpeed * 0.6f
                        : walkSpeed;

            var motion = _moveDir * speed + Vector3.up * _velocity.y;
            _cc.Move(motion * Time.deltaTime);
        }

        // -------------------------------------------------------------------
        // External hooks (called by weapons, vehicles, etc.)
        public void EquipWeapon(WeaponBase w)
        {
            currentWeapon = w;
            if (w != null) w.transform.SetParent(weaponSocket, false);
        }

        public Transform GetAimPoint() => cameraRoot;

        private void OnDrawGizmosSelected()
        {
            if (cameraRoot != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawRay(cameraRoot.position, cameraRoot.forward * 50f);
            }
        }
    }
}