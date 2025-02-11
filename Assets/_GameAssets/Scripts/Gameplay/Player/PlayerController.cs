using Unity.VisualScripting;
using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public event Action OnPlayerJump;

    [Header("References")]
    [SerializeField] private Transform _orientationTransform;
    [Header("Movement Settings")]
    [SerializeField] private KeyCode _movementKey;
    [SerializeField] private float playerspeed;
    [Header("Jump Settings")]
    [SerializeField] private KeyCode _JumpKey;
    [SerializeField] private float _JumpForce;
    [SerializeField] private float _airMultiplier;
    [SerializeField] private float _airDrag;
    [SerializeField] private bool _canJump;    


    private Rigidbody _playerRigidbody;
    [SerializeField] private float _jumpCoolDown;
    [Header("Ground Check Settings")]
    [SerializeField] private float _playerHeight;
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private float _groundDrag;

    [Header("Sliding Settings")]
    [SerializeField] private KeyCode _slideKey;
    [SerializeField] private float _slideMultiplier;
    [SerializeField] private float _slideDrag;

    private StateController _statecontroller;
    private float _horizontalInput, _verticalInput;
    private Vector3 _movementDirection;
    private bool _isSliding = false;
    private void Awake()
    {
        _statecontroller = GetComponent<StateController>();
        _playerRigidbody = GetComponent<Rigidbody>();
        _playerRigidbody.freezeRotation = true;
        

    }
   
    void Update()
    {
        setInputs();
        setPlayerDrag();
        LimitPlayerSpeed();
        SetStates();
    }
    void FixedUpdate()
    {
        SetPlayerMovement();
    }
    private void setInputs()
    {
        _horizontalInput = Input.GetAxisRaw("Horizontal");
        _verticalInput = Input.GetAxisRaw("Vertical");
        if(Input.GetKeyDown(_slideKey))
        {
            _isSliding = true;
        }
        else if(Input.GetKeyDown(_movementKey))
        {
            _isSliding = false;
        }
        else if(Input.GetKey(_JumpKey) && _canJump && IsGrounded())
        {
            _canJump = false;
            SetPlayerJumping();
            Invoke(nameof(resetJumping),_jumpCoolDown);

        }
    }
    private void SetStates()
    {
        var movementDirection = GetMovementDirection();
        var isGrounded = IsGrounded();
        var currentState = _statecontroller.GetCurrentState();

        var newState = currentState switch
        {
            _ when movementDirection == Vector3.zero && isGrounded && !_isSliding => PlayerState.Idle,
            _ when movementDirection != Vector3.zero && isGrounded && !_isSliding => PlayerState.Move,
            _ when movementDirection != Vector3.zero && isGrounded && _isSliding => PlayerState.Slide,
            _ when movementDirection == Vector3.zero && isGrounded && _isSliding => PlayerState.SlideIdle,
            _ when !_canJump && !isGrounded => PlayerState.Jump,
            _ => currentState


        };
        if(newState!=currentState)
        {
            _statecontroller.ChangeState(newState);
        }
      
    }

    private void SetPlayerMovement()
    {
        
        _movementDirection = _orientationTransform.forward * _verticalInput + _orientationTransform.right * _horizontalInput;
       
        float ForceMultiplier = _statecontroller.GetCurrentState() switch
        {
            PlayerState.Move => 1f,
            PlayerState.Slide => _slideMultiplier,
            PlayerState.Jump => _airMultiplier,
            _ => 1f
        };
        _playerRigidbody.AddForce(_movementDirection.normalized*playerspeed*ForceMultiplier,ForceMode.Force);
    }
    
    private void setPlayerDrag()
    {
        _playerRigidbody.linearDamping = _statecontroller.GetCurrentState() switch
        {
            PlayerState.Move => _groundDrag,
            PlayerState.Slide => _slideDrag,
            PlayerState.Jump => _airDrag,
            _ => _playerRigidbody.linearDamping

        };
       
        
    }
    private void LimitPlayerSpeed()
    {
        Vector3 flatVelocity = new Vector3(_playerRigidbody.linearVelocity.x,0f,_playerRigidbody.linearVelocity.z);

        if(flatVelocity.magnitude > playerspeed)
        {
            Vector3 limitedVelocity = flatVelocity.normalized * playerspeed;
            _playerRigidbody.linearVelocity = new Vector3(limitedVelocity.x,limitedVelocity.y,limitedVelocity.z);

        }
    }
    private void SetPlayerJumping()
    {
        OnPlayerJump?.Invoke();
        
        _playerRigidbody.linearVelocity = new Vector3(_playerRigidbody.linearVelocity.x,0f,_playerRigidbody.linearVelocity.z);
        _playerRigidbody.AddForce(transform.up*_JumpForce,ForceMode.Impulse);
    }
    private void resetJumping()
    {
        _canJump = true;
    }

    private bool IsGrounded()
    {
        return Physics.Raycast(transform.position,Vector3.down,_playerHeight*0.5f+0.2f,_groundLayer);
    }
    

    private Vector3 GetMovementDirection()
    {
        return _movementDirection.normalized;
    }
}
