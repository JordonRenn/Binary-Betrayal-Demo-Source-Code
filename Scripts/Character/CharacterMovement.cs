using UnityEngine;
using System.Collections;
using GlobalEvents;

/* 
    First Person Controller Hierarchy:
    
    **Game Object Name (Script Name)**

    - Character Controller (CharacterMovement.cs)						<--- THIS SCRIPT
        - FPS_Cam (FirstPersonCamController.cs + CamShake.cs)           
            - FPS System (FPSS_Main.cs)
                - FPS_Interaction (FirstPersonInteraction.cs)           
                - FPS_WeaponObjectPool (FPSS_Pool.cs)                  
                    - POS_GUN_AUDIO
                    - 0_0_Ak-47 (Gun_AK47.cs)
                        - AK_47
                            - MuzzleFlash (MuzzleFlash.cs)
                    - 0_1_SniperRifle (FPSS_WeaponSlotObject.cs)        // Need to make "Gun_SniperRifle.cs"
                    - 1_0_HandGun (Gun_HandGun.cs)
                        - HandGun
                            - MuzzleFlash (MuzzleFlash.cs)
                    - 1_1_ShotGun (FPSS_WeaponSlotObject.cs)            // Need to make "Gun_ShotGun.cs"
                    - 2_0_Knife (FPSS_WeaponSlotObject.cs)              // Need to make "Melee_Knife.cs"
                    - 3_0_Grenade (FPSS_WeaponSlotObject.cs)            // Need to make "Grenade.cs"
                    - 3_1_FlashGrenade (FPSS_WeaponSlotObject.cs)       // Need to make "FlashGrenade.cs"
                    - 3_2_SmokeGrenade (FPSS_WeaponSlotObject.cs)       // Need to make "SmokeGrenade.cs"
                    - 4_0_Unarmed (FPSS_WeaponSlotObject.cs)            // Need to make "Unarmed.cs"
 */

[RequireComponent(typeof(CharacterController))]
[SelectionBase]

public class CharacterMovement : MonoBehaviour
{
    //public static CharacterMovement Instance { get; private set; }

    [SerializeField] private CharacterController controller;

    [Header("Movement Settings")]
    [Space(10)]

	public float moveSpeed = 7.0f;                      // Ground move speed
    public float slowWalkSpeedMultiplier = 0.5f;        // Multiplier for slow walking speed
	public float runAcceleration = 14f;                 // Ground accel
	public float runDeacceleration = 10f;               // Deacceleration that occurs when running on the ground
	public float airAcceleration = 2.0f;                // Air accel
	public float airDeacceleration = 2.0f;              // Deacceleration experienced when opposite strafing
	public float airControl = 0.3f;                     // How precise air control is
    public float airStrafeInfluence = 100f;            // How much mouse movement affects air strafing
	public float sideStrafeAcceleration = 50f;          // How fast acceleration occurs to get up to sideStrafeSpeed when side strafing
	public float sideStrafeSpeed = 1f;                  // What the max speed to generate when side strafing
	public float jumpSpeed = 8.0f;
	public float friction = 6f;
	private float playerTopVelocity = 0;
	public float playerFriction = 0f;

    //private variables

    private float wishspeed2;
	private float gravity = -20f;
	private float wishspeed;

	private float addspeed;
	private float accelspeed;
	private float currentspeed;
	private float zspeed;
	private float speed;
	private float dot;
	private float k;
	private float accel;
	private float newspeed;
	private float control;
	private float drop;

    private bool JumpQueue = false;
	private bool wishJump = false;
    private float jumpBufferTimer = 0f;
    private float jumpBufferDuration = 0.1f; // Time window to allow another jump

    //

    //private Vector3 moveDirection;
	private Vector3 moveDirectionNorm;
	private Vector3 playerVelocity;
	Vector3 wishdir;
	Vector3 vec;
    //private float previousMouseX = 0f; // For tracking mouse movement
    private float moveX;
    private float moveZ;

	private Vector3 udp;

    [Header("Ground Check")]
    [Space(10)]

    private bool grounded;
    [SerializeField] private LayerMask whatIsGround;
    [SerializeField] private float groundCheckBuffer = 0.2f;

    [Header("Dev Options")]
    [Space(10)]

    [SerializeField] private float initDelay = 0f;

    [HideInInspector] public bool moveDisabled = false; 		//needs to be true for teleporting to work
	[SerializeField] private bool logJumpMetrics = false; 		//for testing jump metrics

    // Jump tracking variables
    private Vector3 jumpStartPosition;
    private bool wasGroundedLastFrame = true;
    private bool isTrackingJump = false;
    [HideInInspector] public float lastDropDistance = 0f; // Always calculated for future fall damage
	void Awake()
	{
		// Debug.Log("CHARACTER MOVEMENT | Instantiated");

		if (GameMaster.Instance != null)
		{
			GameMaster.Instance.playerObject = this.gameObject;
			// SBGDebug.LogInfo("Registered player object with GameMaster", "CharacterMovement");
		}
	}

	void Start()
	{
		StartCoroutine(Init(initDelay));
		// GameMaster.Instance.gm_PlayerSpawned.Invoke();
    }

    IEnumerator Init(float delay)
    {
        yield return new WaitForSeconds(delay);
		//GameMaster.Instance.gm_PlayerSpawned.Invoke();
		LevelEvents.RaisePlayerControllerInstantiated();
        SubscribeToEvents();
        controller = GetComponent<CharacterController>();
    }

	void SubscribeToEvents()
    {
        InputHandler.Instance.OnJumpInput.AddListener(Jump);
    }

    void Update()
    {
        GroundCheck(); 
        QueueJump();
        TrackJumpMetrics();
    }


    void FixedUpdate()
    {
        if (moveDisabled) {return;} //jank but needed to make teleporting work (sort of), don't ask
        
        /* Movement, here's the important part */
		if (controller.isGrounded)
			GroundMove();
		else if (!controller.isGrounded)
			AirMove();

		// Move the controller
		controller.Move(playerVelocity * Time.deltaTime);

		// Calculate top velocity
		udp = playerVelocity;
		udp.y = 0;
		if (udp.magnitude > playerTopVelocity)
			playerTopVelocity = udp.magnitude;
    }

    

    void QueueJump()
	{
        // Update jump buffer timer
        if (jumpBufferTimer > 0)
        {
            jumpBufferTimer -= Time.deltaTime;
        }

		if (grounded && JumpQueue && jumpBufferTimer > 0)
		{
			wishJump = true;
			JumpQueue = false;
            jumpBufferTimer = 0f;
		}
	}

    void TrackJumpMetrics()
    {
        // Check if player just lost contact with ground (started jumping/falling)
        if (wasGroundedLastFrame && !grounded && !isTrackingJump)
        {
            jumpStartPosition = transform.position;
            isTrackingJump = true;
        }
        
        // Check if player just landed (regained contact with ground)
        if (!wasGroundedLastFrame && grounded && isTrackingJump)
        {
            Vector3 landingPosition = transform.position;
            CalculateJumpMetrics(jumpStartPosition, landingPosition);
            isTrackingJump = false;
        }
        
        // Update previous frame state
        wasGroundedLastFrame = grounded;
    }

    void CalculateJumpMetrics(Vector3 startPos, Vector3 endPos)
    {
        // Calculate horizontal distance (jump length)
        Vector3 horizontalMovement = new Vector3(endPos.x - startPos.x, 0, endPos.z - startPos.z);
        float jumpLength = horizontalMovement.magnitude;
        
        // Calculate vertical distance (jump height = positive, drop distance = negative)
        float verticalDistance = endPos.y - startPos.y;
        float jumpHeight = Mathf.Max(0, verticalDistance); // Only positive values for jump height
        float dropDistance = Mathf.Max(0, -verticalDistance); // Only positive values for drop distance
        
        // Always store drop distance for future fall damage calculations
        lastDropDistance = dropDistance;
        
        // Optional: Log combined metrics if any logging is enabled
        if (logJumpMetrics && (jumpLength > 0.1f || jumpHeight > 0.1f || dropDistance > 0.1f))
        {
            // SBGDebug.Log($"[Jump Metrics] Start: {startPos}, End: {endPos}, Length: {jumpLength:F2}, Height: {jumpHeight:F2}, Drop: {dropDistance:F2}", "CharacterMovement | CalculateJumpMetrics");
        }
    }

    void Jump()
    {
        if (grounded && jumpBufferTimer <= 0)
		{
			wishJump = true;
            jumpBufferTimer = 0f; // Reset timer on successful jump
		}

		if (!grounded)
		{
			JumpQueue = true;
            jumpBufferTimer = jumpBufferDuration; // Start buffer timer
		}
    }

    

    //GROUND MOVEMENT

    public void GroundMove()
	{
		// Do not apply friction if the player is queueing up the next jump
		if (!wishJump)
			ApplyFriction(1.0f);
		else
			ApplyFriction(0);

		SetMovementDir();

		wishdir = new Vector3(InputHandler.Instance.MoveInput.x, 0, InputHandler.Instance.MoveInput.y);

		wishdir = transform.TransformDirection(wishdir);
		
        wishdir.Normalize();
		moveDirectionNorm = wishdir;

		wishspeed = wishdir.magnitude;
		wishspeed *= moveSpeed * (InputHandler.Instance.SlowWalkInput ? slowWalkSpeedMultiplier : 1.0f);

		Accelerate(wishdir, wishspeed, runAcceleration);

		// Reset the gravity velocity
		playerVelocity.y = 0;

		if (wishJump)
		{
			playerVelocity.y = jumpSpeed;
			wishJump = false;
		}

		/**
			* Applies friction to the player, called in both the air and on the ground
			*/
		void ApplyFriction(float t)
		{
			vec = playerVelocity; // Equivalent to: VectorCopy();
			vec.y = 0f;
			speed = vec.magnitude;
			drop = 0f;

			/* Only if the player is on the ground then apply friction */
			if (controller.isGrounded)
			{
				control = speed < runDeacceleration ? runDeacceleration : speed;
				drop = control * friction * Time.deltaTime * t;
			}

			newspeed = speed - drop;
			playerFriction = newspeed;
			if (newspeed < 0)
				newspeed = 0;
			if (speed > 0)
				newspeed /= speed;

			playerVelocity.x *= newspeed;
			// playerVelocity.y *= newspeed;
			playerVelocity.z *= newspeed;
		}
	}

    //AIR MOVEMENT
    public void AirMove()
	{
		SetMovementDir();

        // Get mouse movement delta from the input system and make it frame-rate independent
        float mouseDelta = InputHandler.Instance.LookInput.x;
        float frameIndependentDelta = mouseDelta * Time.fixedDeltaTime * 60f; // Normalize to 60fps feel

		wishdir = new Vector3(InputHandler.Instance.MoveInput.x, 0, InputHandler.Instance.MoveInput.y);

		wishdir = transform.TransformDirection(wishdir);

        // Apply mouse movement influence when strafing
        if (InputHandler.Instance.MoveInput.x != 0 && Mathf.Abs(mouseDelta) > 0.1f)
        {
            // Adjust direction based on mouse movement - strafe in the direction you're looking
            float mouseInfluence = Mathf.Sign(InputHandler.Instance.MoveInput.x) * frameIndependentDelta * airControl * airStrafeInfluence; // Configurable scale factor
            wishdir = Quaternion.Euler(0, mouseInfluence, 0) * wishdir;
        }

		wishspeed = wishdir.magnitude;
		wishspeed *= moveSpeed * (InputHandler.Instance.SlowWalkInput ? slowWalkSpeedMultiplier : 1.0f);

		wishdir.Normalize();
		moveDirectionNorm = wishdir;

		// Aircontrol
		wishspeed2 = wishspeed;
		if (Vector3.Dot(playerVelocity, wishdir) < 0)
			accel = airDeacceleration;
		else
			accel = airAcceleration;

		// If the player is ONLY strafing left or right
		if (InputHandler.Instance.MoveInput.x == 0 && InputHandler.Instance.MoveInput.y != 0)
		{
			if (wishspeed > sideStrafeSpeed)
				wishspeed = sideStrafeSpeed;
			accel = sideStrafeAcceleration;
		}

		Accelerate(wishdir, wishspeed, accel);

		AirControl(wishdir, wishspeed2);

		// !Aircontrol

		// Apply gravity
		playerVelocity.y += gravity * Time.deltaTime;

		/**
			* Air control occurs when the player is in the air, it allows
			* players to move side to side much faster rather than being
			* 'sluggish' when it comes to cornering.
			*/

		void AirControl(Vector3 wishdir, float wishspeed)
		{
			// Can't control movement if not moving forward or backward
			if (InputHandler.Instance.MoveInput.x == 0 || wishspeed == 0)
				return;

			zspeed = playerVelocity.y;
			playerVelocity.y = 0;
			/* Next two lines are equivalent to idTech's VectorNormalize() */
			speed = playerVelocity.magnitude;
			playerVelocity.Normalize();

			dot = Vector3.Dot(playerVelocity, wishdir);
			k = 32;
			k *= airControl * dot * dot * Time.deltaTime;

			// Change direction while slowing down
			if (dot > 0)
			{
				playerVelocity.x = playerVelocity.x * speed + wishdir.x * k;
				playerVelocity.y = playerVelocity.y * speed + wishdir.y * k;
				playerVelocity.z = playerVelocity.z * speed + wishdir.z * k;

				playerVelocity.Normalize();
				moveDirectionNorm = playerVelocity;
			}

			playerVelocity.x *= speed;
			playerVelocity.y = zspeed; // Note this line
			playerVelocity.z *= speed;

		}
	}

    public void Accelerate(Vector3 wishdir, float wishspeed, float accel)
	{
		currentspeed = Vector3.Dot(playerVelocity, wishdir);
		addspeed = wishspeed - currentspeed;
		if (addspeed <= 0)
			return;
		accelspeed = accel * Time.deltaTime * wishspeed;
		if (accelspeed > addspeed)
			accelspeed = addspeed;

		playerVelocity.x += accelspeed * wishdir.x;
		playerVelocity.z += accelspeed * wishdir.z;
	}

    public void SetMovementDir()
	{
		moveX = InputHandler.Instance.MoveInput.x;
		moveZ = InputHandler.Instance.MoveInput.y;
	}

    void GroundCheck()
    {
        float checkHeight = controller.height;
        grounded = Physics.Raycast(transform.position, Vector3.down, checkHeight * 0.5f + groundCheckBuffer, whatIsGround);
    }
}
