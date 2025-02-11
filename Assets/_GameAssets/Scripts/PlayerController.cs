using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem;

public class NewMonoBehaviourScript : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform _orientationTransform;
    [Header("Movement Settings")]
    [SerializeField] private KeyCode _movementKey;
    [SerializeField] private float playerspeed;
    [Header("Jump Settings")]
    [SerializeField] private KeyCode _JumpKey;
    [SerializeField] private float _JumpForce;
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
    private float _horizontalInput, _verticalInput;
    private Vector3 _movementDirection;
    private bool _isSliding = false;
    private void Awake()
    {
        _playerRigidbody = GetComponent<Rigidbody>();
        _playerRigidbody.freezeRotation = true;

    }
    void Update()
    {
        setInputs();
        setPlayerDrag();
        LimitPlayerSpeed();
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
            Debug.Log("Player Sliding!");

        }
        else if(Input.GetKeyDown(_movementKey))
        {
            _isSliding = false;
            Debug.Log("Player Moving!");
        }
        else if(Input.GetKey(_JumpKey) && _canJump && IsGrounded())
        {
            _canJump = false;
            SetPlayerJumping();
            Invoke(nameof(resetJumping),_jumpCoolDown);

        }
    }
    private void SetPlayerMovement()
    {
        _movementDirection = _orientationTransform.forward * _verticalInput + _orientationTransform.right * _horizontalInput;
        
        if (_isSliding)
        {
            _playerRigidbody.AddForce(_movementDirection.normalized*playerspeed*_slideMultiplier,ForceMode.Force);
        }
        else if (_isSliding == false)
        {
            _playerRigidbody.AddForce(_movementDirection.normalized*playerspeed,ForceMode.Force);
            Debug.Log("Player Moving!2");
        }

    
    }
    
    private void setPlayerDrag()
    {
        if(_isSliding)
        {
            _playerRigidbody.linearDamping = _slideDrag;

        }
        else
        {
            _playerRigidbody.linearDamping = _groundDrag;

        }
        
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
    
}
