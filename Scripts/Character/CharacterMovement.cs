using UnityEngine;
using System.Collections;
using DG.Tweening;
using FMODUnity;

/* 
Based on script by Alpharaoh
Source:
https://github.com/alpharaoh/SourceMovement/blob/main/PlayerMovementWithStrafes.cs
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
	public float runAcceleration = 14f;                 // Ground accel
	public float runDeacceleration = 10f;               // Deacceleration that occurs when running on the ground
	public float airAcceleration = 2.0f;                // Air accel
	public float airDeacceleration = 2.0f;              // Deacceleration experienced when opposite strafing
	public float airControl = 0.3f;                     // How precise air control is
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

    //

    private Vector3 moveDirection;
	private Vector3 moveDirectionNorm;
	private Vector3 playerVelocity;
	Vector3 wishdir;
	Vector3 vec;

	private float x;
	private float z;

	private Vector3 udp;

    [Header("Ground Check")]
    [Space(10)]

    private bool grounded;
    [SerializeField] private LayerMask whatIsGround;
    [SerializeField] private float groundCheckBuffer = 0.2f;

    [Header("Dev Options")]
    [Space(10)]

    [SerializeField] private float initDelay = 0.1f;

    [HideInInspector] public bool moveDisabled = false; //needs to be true for teleporting to work

	
    void Start()
    {
        StartCoroutine(Init(initDelay));
    }

    IEnumerator Init(float delay)
    {
        yield return new WaitForSeconds(delay);
		GameMaster.Instance.gm_PlayerSpawned.Invoke();
        SubscribeToEvents();
        controller = GetComponent<CharacterController>();
    }

	void SubscribeToEvents()
    {
        FPS_InputHandler.Instance.jumpTriggered.AddListener(Jump);
    }

    void Update()
    {
        GroundCheck(); 
        QueueJump();
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
		if (grounded && JumpQueue)
		{
			wishJump = true;
			JumpQueue = false;
		}
	}

    void Jump()
    {
        if (grounded)
		{
			wishJump = true;
		}

		if (!grounded)
		{
			JumpQueue = true;
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

		wishdir = new Vector3(FPS_InputHandler.Instance.MoveInput.x, 0, FPS_InputHandler.Instance.MoveInput.y);

		wishdir = transform.TransformDirection(wishdir);
		
        wishdir.Normalize();
		moveDirectionNorm = wishdir;

		wishspeed = wishdir.magnitude;
		wishspeed *= moveSpeed;

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

		wishdir = new Vector3(FPS_InputHandler.Instance.MoveInput.x, 0, FPS_InputHandler.Instance.MoveInput.y);

		wishdir = transform.TransformDirection(wishdir);

		wishspeed = wishdir.magnitude;

		wishspeed *= 7f;

		wishdir.Normalize();
		moveDirectionNorm = wishdir;

		// Aircontrol
		wishspeed2 = wishspeed;
		if (Vector3.Dot(playerVelocity, wishdir) < 0)
			accel = airDeacceleration;
		else
			accel = airAcceleration;

		// If the player is ONLY strafing left or right
		if (FPS_InputHandler.Instance.MoveInput.x == 0 && FPS_InputHandler.Instance.MoveInput.y != 0)
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
			if (FPS_InputHandler.Instance.MoveInput.x == 0 || wishspeed == 0)
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
		x = Input.GetAxis("Horizontal");
		z = Input.GetAxis("Vertical");
	}

    void GroundCheck()
    {
        float checkHeight = controller.height;
        grounded = Physics.Raycast(transform.position, Vector3.down, checkHeight * 0.5f + groundCheckBuffer, whatIsGround);
    }
}
