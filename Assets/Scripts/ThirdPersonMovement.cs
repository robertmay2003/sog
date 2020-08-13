using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

[RequireComponent(typeof(CharacterController))]
public class ThirdPersonMovement : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 4f;
    public float sprintSpeed = 8f;
    public float acceleration = 1f;

    [Space] 
    public float jumpSpeed = 5;
    public float airControl = 0.2f;
    public float gravityScale = 1f;

    public Transform cam;
    
    public Vector2 Movement
    {
        get
        {
            return Vector2.Lerp(
                _movement * (speed / sprintSpeed),
                _movement,
                _sprintAmount
            );
        }
    }

    public float VerticalSpeed => _verticalVelocity;
    public bool IsResting => _isResting;

    private bool _isResting;
    private bool _isSprinting;

    private float _turnSmoothVelocity;
    private float _verticalVelocity;
    private float _sprintAmount;

    private Vector2 _movement;
    private Vector2 _movementSmoothVelocity;
    private Vector2 _input;
    private Quaternion _camAngle;
    
    private CharacterController _controller;
    private InputMaster _controls;

    void Awake()
    {
        _controls = new InputMaster();
    }
    
    // Start is called before the first frame update
    void Start()
    {
        _controller = GetComponent<CharacterController>();
        
        _controls.Player.Movement.performed += OnMove;
        _controls.Player.Movement.canceled += OnMoveCanceled;
        _controls.Player.Jump.performed += OnJump;
        _controls.Player.Sprint.performed += OnSprint;
        _controls.Player.Sprint.canceled += OnSprintCanceled;
        _controls.Player.Rest.performed += OnRest;
        _controls.Player.Rest.canceled += OnRestCanceled;
    }

    void OnDestroy()
    {
        _controls.Player.Movement.performed -= OnMove;
        _controls.Player.Movement.canceled -= OnMoveCanceled;
        _controls.Player.Jump.performed -= OnJump;
        _controls.Player.Sprint.performed -= OnSprint;
        _controls.Player.Sprint.canceled -= OnSprintCanceled;
        _controls.Player.Rest.performed -= OnRest;
        _controls.Player.Rest.canceled -= OnRestCanceled;
    }

    void OnEnable()
    {
        _controls.Enable();
    }

    void OnDisable()
    {
        _controls.Disable();
    }

    // Update is called once per frame
    void Update()
    {
        // Calculate smoothed movement vector
        if (_controller.isGrounded && !_isResting) // Player on ground
        {
            _movement = Vector2.SmoothDamp(
                _movement, 
                _input, 
                ref _movementSmoothVelocity,
                1/acceleration * Time.deltaTime
            );
        }
        else if (_isResting) // Player resting
        {
            _movement = Vector2.SmoothDamp(
                _movement,
                Vector2.zero,
                ref _movementSmoothVelocity,
                1/acceleration * Time.deltaTime
            );
        }
        else // Player in air
        {
            _movement = Vector2.SmoothDamp(
                _movement, 
                _movement + ((_input - _movement) * airControl),
                ref _movementSmoothVelocity,
                1/acceleration * Time.deltaTime
            );
        }

        // Calculate smoothed sprint amount and camera angle
        _sprintAmount = Mathf.Lerp(_sprintAmount, _isSprinting ? 1 : 0, acceleration);
        _camAngle = Quaternion.Slerp(_camAngle, cam.rotation, 1/acceleration * Time.deltaTime);

        // Calculate movement direction from angle and input
        Vector3 direction = new Vector3(_movement.x,0f, _movement.y);
        if (direction.magnitude >= 0.1f)
        {
            float targetAngle = (Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg)
                                + _camAngle.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(
                transform.eulerAngles.y, 
                targetAngle, 
                ref _turnSmoothVelocity,
                1 / acceleration * Time.deltaTime
            );
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 moveDirection = transform.rotation
                                    * Vector3.forward
                                    * direction.magnitude;
            direction = moveDirection;
        }

        // Calculate speed depending on sprint input
        float moveSpeed = Mathf.Lerp(speed, sprintSpeed, _sprintAmount);
        direction *= (moveSpeed * Time.deltaTime);
        
        // Add gravity
        ApplyGravity();
        direction.y = _verticalVelocity * Time.deltaTime;
        
        // Move
        _controller.Move(direction);
    }

    void ApplyGravity()
    {
        if (_controller.isGrounded)
        {
            _verticalVelocity = Mathf.Max(_verticalVelocity, -1f);
        }
        else
        {
            _verticalVelocity += gravityScale * Physics.gravity.y * Time.deltaTime;
        }
    }

    void Jump()
    {
        _verticalVelocity = jumpSpeed;
    }

    void OnMove(InputAction.CallbackContext ctx)
    {
        _input = ctx.ReadValue<Vector2>();
    }

    void OnMoveCanceled(InputAction.CallbackContext ctx)
    {
        _input = Vector2.zero;
    }

    void OnJump(InputAction.CallbackContext ctx)
    {
        // Schedule jump
        if (_controller.isGrounded && !_isResting) Invoke(nameof(Jump), 0.2f);
    }

    void OnSprint(InputAction.CallbackContext ctx)
    {
        _isSprinting = true;
    }

    void OnSprintCanceled(InputAction.CallbackContext ctx)
    {
        _isSprinting = false;
    }

    void OnRest(InputAction.CallbackContext ctx)
    {
        _isResting = true;
    }

    void OnRestCanceled(InputAction.CallbackContext ctx)
    {
        _isResting = false;
    }
}
