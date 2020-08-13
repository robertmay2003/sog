using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(ThirdPersonMovement))]
public class DogAnimationController : MonoBehaviour
{
    private bool _isGrounded;
    private bool _isResting;
    private CharacterController _controller;
    private ThirdPersonMovement _thirdPerson;
    private InputMaster _controls;
    private Animator _animator;
    
    private static readonly int AnimatorSpeed = Animator.StringToHash("speed");
    private static readonly int AnimatorJump = Animator.StringToHash("jump");
    private static readonly int AnimatorGrounded = Animator.StringToHash("grounded");
    private static readonly int AnimatorResting = Animator.StringToHash("resting");
    private static readonly int AnimatorVerticalSpeed = Animator.StringToHash("verticalSpeed");

    void Awake()
    {
        _controls = new InputMaster();
    }
    
    // Start is called before the first frame update
    void Start()
    {
        _controller = GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();
        _thirdPerson = GetComponent<ThirdPersonMovement>();

        _controls.Player.Jump.performed += OnJump;
    }

    void OnDestroy()
    {
        _controls.Player.Jump.performed -= OnJump;
    }

    void OnEnable()
    {
        _controls.Enable();
    }

    void OnDisable()
    {
        _controls.Disable();
    }

    void Update()
    {
        _animator.SetFloat(AnimatorSpeed, _thirdPerson.Movement.magnitude);

        if (_isResting != _thirdPerson.IsResting)
        {
            _isResting = _thirdPerson.IsResting;
            _animator.SetBool(AnimatorResting, _isResting);
        }

        if (_isGrounded != _controller.isGrounded)
        {
            _isGrounded = _controller.isGrounded;
            _animator.SetBool(AnimatorGrounded, _isGrounded);
        }

        if (!_isGrounded)
        {
            _animator.SetFloat(AnimatorVerticalSpeed, _thirdPerson.VerticalSpeed);
        }
    }

    void OnJump(InputAction.CallbackContext ctx)
    {
        if (_controller.isGrounded && !_thirdPerson.IsResting) _animator.SetTrigger(AnimatorJump);
    }
}
