using UnityEngine;
using System.Collections;
using DG.Tweening;
using FMODUnity;
using System;

/// <summary>
/// Character controller for the FPS system.
/// </summary>
public class FPSS_CharacterController : MonoBehaviour
{
    public static FPSS_CharacterController Instance { get; private set; }
    [SerializeField] Transform orientation;
    private FPS_InputHandler input;
    
    [Header("Movement")]
    [Space(10)] 

    public static float moveSpeed;
    [SerializeField] float sprintSpeed;
    [SerializeField] float walkSpeed;
    [SerializeField] float groundDrag;
    private const float forceMultiplier = 10f;

    [Space(5)]

    [SerializeField] float jumpForce;
    [SerializeField] float jumpCooldown;
    [SerializeField] float airMultiplier;
    bool readyToJump = true;
    [Space(5)]

    [Header("Ground Check")]
    [Space(10)] 

    [SerializeField] float playerHeight;
    [SerializeField] float groundCheckBuffer = 0.2f;
    [SerializeField] LayerMask whatIsGround;
    bool grounded;

    [Header("Slope Handling")]
    [Space(10)] 

    [SerializeField] float maxSlopeAngle = 45f;
    private bool onSlope;
    private Vector3 slopeMoveDirection;
    private RaycastHit slopeHit;

    [Header("Crouch Settings")]
    [Space(10)] 

    [SerializeField] private float crouchHeight = 1.5f;
    [SerializeField] private float crouchSpeed;
    [SerializeField] private float crouchTransitionSpeed = 1f;
    private bool isCrouching = false;
    private CapsuleCollider playerCollider;
    private Vector3 initialColliderCenter;

    [Header("SFX")]
    [Space(10)] 

    [SerializeField] private Transform defaultAudioPosition;
    [SerializeField] private EventReference sfx_player_footstep;
    private float baseFootStepRate = 1f;
    [SerializeField] private Transform leftFootstepPosition;
    [SerializeField] private Transform rightFootstepPosition;
    [SerializeField] private EventReference sfx_player_jump;
    [SerializeField] private EventReference sfx_player_land;

    private Rigidbody playerRigidbody;
    private Vector2 moveInput;
    private bool footToggle;
    private bool nextStep;

    /// <summary>
    /// Initializes the character controller.
    /// </summary>
    void Start()
    {
        Instance = this;
        
        moveInput = FPS_InputHandler.Instance.MoveInput;
        input = FPS_InputHandler.Instance;
        
        playerRigidbody = GetComponent<Rigidbody>();
        playerRigidbody.freezeRotation = true;

        playerCollider = GetComponentInChildren<CapsuleCollider>();
        initialColliderCenter = playerCollider.center;

        moveSpeed = walkSpeed;
        footToggle = true;
        nextStep = true;
    }

    void Update()
    {
        SpeedControl();
        GroundCheck();
        CalculateDrag();
        CheckSlope();
        CalculateFootSteps();
        
        moveInput = FPS_InputHandler.Instance.MoveInput;
        moveSpeed = Mathf.Lerp(moveSpeed, input.SlowWalkInput || isCrouching ? walkSpeed : sprintSpeed, Time.deltaTime * 10f);
    }

    void FixedUpdate()
    {
        MovePlayer();
    }
    
    /// <summary>
    /// Moves the player based on input and physics.
    /// </summary>
    private void MovePlayer()
    {
        Vector3 moveDirection = orientation.forward * moveInput.y + orientation.right * moveInput.x;
        Vector3 forceDirection = moveDirection.normalized * moveSpeed * forceMultiplier;

        if (onSlope)
        {
            slopeMoveDirection = Vector3.ProjectOnPlane(moveDirection, slopeHit.normal);
            forceDirection = slopeMoveDirection.normalized * moveSpeed * forceMultiplier;
        }

        if (!grounded)
        {
            forceDirection *= airMultiplier;
        }

        playerRigidbody.AddForce(forceDirection, ForceMode.Force);
    }

    /// <summary>
    /// Makes the player jump if they are grounded and ready to jump.
    /// </summary>
    public void Jump()
    {
        if (readyToJump && grounded)
        {
            readyToJump = false;
            PlaySfx(sfx_player_jump, defaultAudioPosition);
            playerRigidbody.linearVelocity = new Vector3(playerRigidbody.linearVelocity.x, 0f, playerRigidbody.linearVelocity.z);
            playerRigidbody.AddForce(transform.up * jumpForce, ForceMode.Impulse);
            StartCoroutine(ResetJumpAfterDelay());
        }
    }

    /// <summary>
    /// Resets the jump after a delay.
    /// </summary>
    private IEnumerator ResetJumpAfterDelay()
    {
        yield return new WaitForSeconds(jumpCooldown);
        readyToJump = true;
    }

    /// <summary>
    /// Starts the crouch state.
    /// </summary>
    public void StartCrouch()
    {
        isCrouching = true;
        playerCollider.DOComplete();
        DOVirtual.Float(playerCollider.height, crouchHeight, crouchTransitionSpeed, value =>
        {
            playerCollider.height = value;
        });
        DOVirtual.Float(playerCollider.center.y, initialColliderCenter.y, crouchTransitionSpeed, value =>
        {
            playerCollider.center = new Vector3(initialColliderCenter.x, value, initialColliderCenter.z);
        });

        if (grounded)
        {
            moveSpeed = walkSpeed;
            StartCoroutine(ApplyDownwardForce());
        }
    }

    /// <summary>
    /// Stops the crouch state.
    /// </summary>
    public void StopCrouch()
    {
        isCrouching = false;
        playerCollider.DOComplete();
        DOVirtual.Float(playerCollider.height, playerHeight, crouchTransitionSpeed, value =>
        {
            playerCollider.height = value;
        });
        DOVirtual.Float(playerCollider.center.y, initialColliderCenter.y, crouchTransitionSpeed, value =>
        {
            playerCollider.center = new Vector3(initialColliderCenter.x, value, initialColliderCenter.z);
        });
        moveSpeed = walkSpeed;
    }

    /// <summary>
    /// Applies a downward force while crouching.
    /// </summary>
    private IEnumerator ApplyDownwardForce()
    {
        while (isCrouching && grounded)
        {
            playerRigidbody.AddForce(Vector3.down * 100f, ForceMode.Force);
            yield return null;
        }
    }

    /// <summary>
    /// Calculates the drag based on whether the player is grounded.
    /// </summary>
    void CalculateDrag()
    {
        float targetDrag = grounded ? groundDrag : 0f;
        playerRigidbody.linearDamping = Mathf.Lerp(playerRigidbody.linearDamping, targetDrag, Time.deltaTime * 10f);
    }

    /// <summary>
    /// Checks if the player is on a slope.
    /// </summary>
    void CheckSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + groundCheckBuffer, whatIsGround))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            onSlope = angle < maxSlopeAngle && angle > 0;
        }
        else
        {
            onSlope = false;
        }
    }

    /// <summary>
    /// Controls the player's speed to ensure it does not exceed the maximum speed.
    /// </summary>
    void SpeedControl()
    {
        Vector3 flatVelocity = new Vector3(playerRigidbody.linearVelocity.x, 0f, playerRigidbody.linearVelocity.z);

        if(flatVelocity.magnitude > moveSpeed)
        {
            Vector3 limitedVelocity = Vector3.ClampMagnitude(flatVelocity, moveSpeed);
            playerRigidbody.linearVelocity = new Vector3(limitedVelocity.x, playerRigidbody.linearVelocity.y, limitedVelocity.z);
        }
    }

    
    void GroundCheck()
    {
        float checkHeight = isCrouching ? crouchHeight : playerHeight;                                                                  // Check height based on crouching state
        grounded = Physics.Raycast(transform.position, Vector3.down, checkHeight * 0.5f + groundCheckBuffer, whatIsGround);             // Check if the player is grounded
        //TODO: Implement gound type check
        Debug.DrawLine(transform.position, transform.position + Vector3.down * (checkHeight * 0.5f + groundCheckBuffer), Color.red);    // Draw a line to visualize the ground check
    }
    

    

    void CalculateFootSteps() //used to calculate speed of footstep sounds and play back and forth between left and right foot
    {
        bool isMovingOnGround;
        
        if (playerRigidbody.linearVelocity.magnitude > 0.1f && grounded)
        {
            isMovingOnGround = true;

            if (isMovingOnGround && nextStep)
            {
                nextStep = false;
                
                if (moveSpeed <= walkSpeed + 1)
                {
                    StartCoroutine(FootStep(baseFootStepRate));
                }
                else if (moveSpeed >= sprintSpeed - 1)
                {
                    StartCoroutine(FootStep(baseFootStepRate / 2));
                }
            }
        }

        isMovingOnGround = false;
    }

    /// <summary>
    /// Plays the footstep sound with a delay.
    /// </summary>
    private IEnumerator FootStep(float stepDelay)
    {
        if (footToggle)
        {
            PlaySfx(sfx_player_footstep, rightFootstepPosition);
            footToggle = !footToggle;
            yield return new WaitForSeconds(stepDelay);
            nextStep = true;
        }
        else
        {
            PlaySfx(sfx_player_footstep, leftFootstepPosition);
            footToggle = !footToggle;
            yield return new WaitForSeconds(stepDelay);
            nextStep = true;
        }
    }

    /// <summary>
    /// Plays a sound effect at the specified position.
    /// </summary>
    /// <param name="eventRef">The sound effect to play.</param>
    /// <param name="audioPos">The position to play the sound effect at.</param>
    public void PlaySfx(EventReference eventRef, Transform audioPos)
    {
        RuntimeManager.PlayOneShot(eventRef, audioPos.position);
    }
}