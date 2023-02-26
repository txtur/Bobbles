using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{
	//Scriptable object which holds all the player's movement parameters
	public PlayerData Data;

	#region COMPONENTS
    public Rigidbody2D RB { get; private set; }
	//Script to handle player animations
	public PlayerAnimator anim { get; private set; }
	private PlayerInput playerInput;
	#endregion

	#region STATE PARAMETERS
	public bool IsFacingRight { get; set; }
	public bool IsJumping { get; set; }
	public bool IsWallJumping { get; set; }
	public bool IsRolling { get; set; }
	public bool isCrouching { get; set; }
	public bool isGrounded;
	public bool isSleep;
	public bool isAwake;

	//Timers
	public float LastOnGroundTime { get; private set; }
	public float LastOnWallTime { get; private set; }
	public float LastOnWallRightTime { get; private set; }
	public float LastOnWallLeftTime { get; private set; }

	//Jump
	public bool _isJumpCut;
	public bool _isJumpFalling;
	private int funnyJump;

	//Wall Jump
	private float _wallJumpStartTime;
	private int _lastWallJumpDir;

	//Dash
	public int _rollsLeft;
	private bool _rollRefilling;
	public int _lastRollDir;
	private float lastRollTime;

	public bool stopRoll = false;

	#endregion

	#region INPUT PARAMETERS
	private Vector2 _moveInput;

	public float LastPressedJumpTime { get; private set; }
	public float LastPressedRollTime { get; private set; }
	public float LastPressedCrouchTime { get; private set; }
	#endregion

	#region CHECK PARAMETERS
	//Set all of these up in the inspector
	[Header("Checks")] 
	[SerializeField] private Transform _groundCheckPoint;
	//Size of groundCheck depends on the size of your character generally you want them slightly small than width (for ground) and height (for the wall check)
	[SerializeField] private Vector2 _groundCheckSize = new Vector2(0.49f, 0.03f);
	[Space(5)]
	[SerializeField] private Transform _frontWallCheckPoint;
	[SerializeField] private Transform _backWallCheckPoint;
	[SerializeField] private Vector2 _wallCheckSize = new Vector2(0.5f, 1f);
    #endregion

    #region LAYERS & TAGS
    [Header("Layers & Tags")]
	[SerializeField] private LayerMask _groundLayer;
	#endregion

    private void Awake()
	{
		RB = GetComponent<Rigidbody2D>();
		anim = GetComponent<PlayerAnimator>();
	}

	private void Start()
	{
		playerInput = GetComponent<PlayerInput>();
		SetGravityScale(Data.gravityScale);
		IsFacingRight = true;

		Scene activeScene = SceneManager.GetActiveScene();
		if (activeScene.name == "1.SoftRains")
		{
			isSleep = true;
		}
	}

	private void Update()
	{
        #region TIMERS
        LastOnGroundTime -= Time.deltaTime;
		LastOnWallTime -= Time.deltaTime;
		LastOnWallRightTime -= Time.deltaTime;
		LastOnWallLeftTime -= Time.deltaTime;

		LastPressedJumpTime -= Time.deltaTime;
		LastPressedRollTime -= Time.deltaTime;
		lastRollTime -= Time.deltaTime;
		#endregion

		#region INPUT HANDLER
		Vector2 input = playerInput.actions["Move"].ReadValue<Vector2>();
		_moveInput.x = input.x;
		_moveInput.y = input.y;

		if (_moveInput.x != 0)
			CheckDirectionToFace(_moveInput.x > 0);

		if(playerInput.actions["Jump"].triggered)
        {
			OnJumpInput();
        }

		if (playerInput.actions["Jump"].WasReleasedThisFrame())
		{
			OnJumpUpInput();
		}

		if (playerInput.actions["Roll"].triggered)
		{
			OnRollInput();
		}

		#endregion

		#region COLLISION CHECKS
		if (!IsRolling && !IsJumping)
		{
			//Ground Check
			if (Physics2D.OverlapBox(_groundCheckPoint.position, _groundCheckSize, 0, _groundLayer) && !IsJumping)
			{
				if(LastOnGroundTime < -0.1f)
                {
					anim.justLanded = true;
                }

				LastOnGroundTime = Data.coyoteTime; //if so sets the lastGrounded to coyoteTime
				isGrounded = true;
            }		

			//Right Wall Check
			if (((Physics2D.OverlapBox(_frontWallCheckPoint.position, _wallCheckSize, 0, _groundLayer) && IsFacingRight)
				|| (Physics2D.OverlapBox(_backWallCheckPoint.position, _wallCheckSize, 0, _groundLayer) && !IsFacingRight)) && !IsWallJumping)
				LastOnWallRightTime = Data.coyoteTime;

			//Left Wall Check
			if (((Physics2D.OverlapBox(_frontWallCheckPoint.position, _wallCheckSize, 0, _groundLayer) && !IsFacingRight)
				|| (Physics2D.OverlapBox(_backWallCheckPoint.position, _wallCheckSize, 0, _groundLayer) && IsFacingRight)) && !IsWallJumping)
				LastOnWallLeftTime = Data.coyoteTime;

			//Two checks needed for both left and right walls since whenever the play turns the wall checkPoints swap sides
			LastOnWallTime = Mathf.Max(LastOnWallLeftTime, LastOnWallRightTime);
		}
		#endregion

		#region JUMP CHECKS
		if (IsJumping && RB.velocity.y < 0)
		{
			IsJumping = false;

			if(!IsWallJumping)
				_isJumpFalling = true;
		}

		if (IsWallJumping && Time.time - _wallJumpStartTime > Data.wallJumpTime)
		{
			IsWallJumping = false;
		}

		if (LastOnGroundTime > 0 && !IsJumping && !IsWallJumping)
        {
			_isJumpCut = false;

			if(!IsJumping)
				_isJumpFalling = false;
		}

		if (!IsRolling)
		{
			//Jump
			if (CanJump() && LastPressedJumpTime > 0)
			{
				IsJumping = true;
				IsWallJumping = false;
				isGrounded = false;
				_isJumpCut = false;
				_isJumpFalling = false;
				Jump();

				funnyJump = Random.Range(1, 1984);

				if (lastRollTime > -0.2f && LastOnGroundTime > -0.3f)
					anim.startedBounce = true;	
				else if (funnyJump == 43)
					anim.startedFunnyJump = true;
				else
					anim.startedJumping = true;
			}
			//WALL JUMP
			else if (CanWallJump() && LastPressedJumpTime > 0)
			{
				IsWallJumping = true;
				IsJumping = false;
				_isJumpCut = false;
				_isJumpFalling = false;

				_wallJumpStartTime = Time.time;
				_lastWallJumpDir = (LastOnWallRightTime > 0) ? -1 : 1;

				WallJump(_lastWallJumpDir);

				anim.startedJumping = true;
			}

			else if (CanCrouch() && LastPressedCrouchTime > 0)
			{
				isCrouching = true;

				anim.startedCrouching = true;
			}
		}
		#endregion

		#region ROLL CHECKS
		if (CanRoll() && LastPressedRollTime > 0)
		{
			//Freeze game for split second. Adds juiciness and a bit of forgiveness over directional input
			Sleep(Data.rollSleepTime); 

			_lastRollDir = IsFacingRight ? 1 : -1;
		
			IsRolling = true;
			IsJumping = false;
			IsWallJumping = false;
			_isJumpCut = false;
			LastOnGroundTime = 0;
			lastRollTime = 0;

			StartCoroutine(nameof(Roll), _lastRollDir);

			anim.startedRolling = true;
		}
		#endregion

		#region GRAVITY
		if (!IsRolling)
		{
			//Higher gravity if we've released the jump input or are falling
			if (RB.velocity.y < 0 && _moveInput.y < 0)
			{
				//Much higher gravity if holding down
				SetGravityScale(Data.gravityScale * Data.fastFallGravityMult);
				//Caps maximum fall speed, so when falling over large distances we don't accelerate to insanely high speeds
				RB.velocity = new Vector2(RB.velocity.x, Mathf.Max(RB.velocity.y, -Data.maxFastFallSpeed));
			}
			else if (_isJumpCut)
			{
				//Higher gravity if jump button released
				SetGravityScale(Data.gravityScale * Data.jumpCutGravityMult);
				RB.velocity = new Vector2(RB.velocity.x, Mathf.Max(RB.velocity.y, -Data.maxFallSpeed));
			}
			else if ((IsJumping || IsWallJumping || _isJumpFalling) && Mathf.Abs(RB.velocity.y) < Data.jumpHangTimeThreshold)
			{
				SetGravityScale(Data.gravityScale * Data.jumpHangGravityMult);
			}
			else if (RB.velocity.y < 0)
			{
				//Higher gravity if falling
				SetGravityScale(Data.gravityScale * Data.fallGravityMult);
				//Caps maximum fall speed, so when falling over large distances we don't accelerate to insanely high speeds
				RB.velocity = new Vector2(RB.velocity.x, Mathf.Max(RB.velocity.y, -Data.maxFallSpeed));
			}
			else
			{
				//Default gravity if standing on a platform or moving upwards
				SetGravityScale(Data.gravityScale);
			}
		}
		else
		{
			//No gravity when rolling
			SetGravityScale(0);
		}
		#endregion
    }

    private void FixedUpdate()
	{
		//Handle Run
		if (IsWallJumping)
			Run(Data.wallJumpRunLerp);
		else if (!IsRolling)
			Run(1);
    }

    #region INPUT CALLBACKS
	//Methods which whandle input detected in Update()
    public void OnJumpInput()
	{
		LastPressedJumpTime = Data.jumpInputBufferTime;
	}

	public void OnJumpUpInput()
	{
		if (CanJumpCut() || CanWallJumpCut())
			_isJumpCut = true;
	}

	public void OnRollInput()
	{
		LastPressedRollTime = Data.rollInputBufferTime;
	}
	
	public void OnCrouchInput()
	{
		LastPressedCrouchTime = Data.crouchInputBufferTime;
	}
    #endregion

    #region GENERAL METHODS
    public void SetGravityScale(float scale)
	{
		RB.gravityScale = scale;
	}

	private void Sleep(float duration)
    {
		//Method used so we don't need to call StartCoroutine everywhere
		StartCoroutine(nameof(PerformSleep), duration);
    }

	private IEnumerator PerformSleep(float duration)
    {
		Time.timeScale = 0;
		yield return new WaitForSecondsRealtime(duration); //Must be Realtime since timeScale with be 0 
		Time.timeScale = 1;
	}
    #endregion

	//MOVEMENT METHODS
    #region RUN METHODS
    private void Run(float lerpAmount)
	{
		//Calculate the direction we want to move in and our desired velocity
		float targetSpeed = _moveInput.x * Data.runMaxSpeed;
		//We can reduce are control using Lerp() this smooths changes to are direction and speed
		targetSpeed = Mathf.Lerp(RB.velocity.x, targetSpeed, lerpAmount);

		#region Calculate AccelRate
		float accelRate;

		//Gets an acceleration value based on if we are accelerating (includes turning) 
		//or trying to decelerate (stop). As well as applying a multiplier if we're air borne.
		if (LastOnGroundTime > 0)
			accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? Data.runAccelAmount : Data.runDeccelAmount;
		else
			accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? Data.runAccelAmount * Data.accelInAir : Data.runDeccelAmount * Data.deccelInAir;
		#endregion

		#region Add Bonus Jump Apex Acceleration
		//Increase are acceleration and maxSpeed when at the apex of their jump, makes the jump feel a bit more bouncy, responsive and natural
		if ((IsJumping || IsWallJumping || _isJumpFalling) && Mathf.Abs(RB.velocity.y) < Data.jumpHangTimeThreshold)
		{
			accelRate *= Data.jumpHangAccelerationMult;
			targetSpeed *= Data.jumpHangMaxSpeedMult;
		}
		#endregion

		#region Conserve Momentum
		//We won't slow the player down if they are moving in their desired direction but at a greater speed than their maxSpeed
		if(Data.doConserveMomentum && Mathf.Abs(RB.velocity.x) > Mathf.Abs(targetSpeed) && Mathf.Sign(RB.velocity.x) == Mathf.Sign(targetSpeed) && Mathf.Abs(targetSpeed) > 0.01f && LastOnGroundTime < 0)
		{
			accelRate = 0; 
		}
		#endregion

		//Calculate difference between current velocity and desired velocity
		float speedDif = targetSpeed - RB.velocity.x;
		float movement = speedDif * accelRate;

		//Convert this to a vector and apply to rigidbody
		RB.AddForce(movement * Vector2.right, ForceMode2D.Force);
	}

	private void Turn()
	{
		//stores scale and flips the player along the x axis, 
		Vector3 scale = transform.localScale; 
		scale.x *= -1;
		transform.localScale = scale;

		IsFacingRight = !IsFacingRight;
	}
    #endregion

    #region JUMP METHODS
    private void Jump()
	{
		//Ensures we can't call Jump multiple times from one press
		LastPressedJumpTime = 0;
		LastOnGroundTime = 0;

		#region Perform Jump
		//Increase the force applied if we are falling
		float force = Data.jumpForce;
		if (lastRollTime > -0.2f && LastOnGroundTime > -0.3f)
		{
			force *= 1.2f;
			IsRolling = false;
			_rollsLeft += Data.rollAmount;
		}
		if (RB.velocity.y < 0)
			force -= RB.velocity.y;

		RB.AddForce(Vector2.up * force, ForceMode2D.Impulse);
		#endregion
	}

	private void WallJump(int dir)
	{
		//Ensures we can't call Wall Jump multiple times from one press
		LastPressedJumpTime = 0;
		LastOnGroundTime = 0;
		LastOnWallRightTime = 0;
		LastOnWallLeftTime = 0;

		#region Perform Wall Jump
		Vector2 force = new Vector2(Data.wallJumpForce.x, Data.wallJumpForce.y);
		force.x *= dir; //apply force in opposite direction of wall

		if (Mathf.Sign(RB.velocity.x) != Mathf.Sign(force.x))
			force.x -= RB.velocity.x;

		if (RB.velocity.y < 0)
			force.y -= RB.velocity.y;

		RB.AddForce(force, ForceMode2D.Impulse);
		#endregion
	}
	#endregion

	#region ROLL METHODS
	public IEnumerator Roll(int dir)
	{
		LastPressedRollTime = 0;
		_rollsLeft--;

		float startTime = Time.time;

		if (isGrounded == false)
			while (Time.time - startTime <= Data.rollAttackTime)
			{
				RB.velocity = new Vector2(Data.rollSpeed * Data.rollAirMult * dir, 8f);
				yield return null;
				
				if (stopRoll)
				{
					startTime = 1;
					StopCoroutine(nameof(Roll));
				}
			}
		else
			while (Time.time - startTime <= Data.rollAttackTime)
			{
				RB.velocity = new Vector2(Data.rollSpeed * dir, 0f);
				yield return null;

				if (stopRoll)
				{
					startTime = 1;
					StopCoroutine(nameof(Roll));
				}
			}
		IsRolling = false;
	}
	private IEnumerator RefillRoll(int amount)
	{
		_rollRefilling = true;
		yield return new WaitForSeconds(Data.rollRefillTime);
		_rollRefilling = false;
		_rollsLeft = Mathf.Min(Data.rollAmount, _rollsLeft + 1);
	}

	#endregion

    #region CHECK METHODS
    public void CheckDirectionToFace(bool isMovingRight)
	{
		if (isMovingRight != IsFacingRight && Time.timeScale != 0f)
			Turn();
	}

    public bool CanJump()
    {
		return LastOnGroundTime > 0 && !IsJumping && Time.timeScale != 0f;
    }

	private bool CanWallJump()
    {
		return LastPressedJumpTime > 0 && LastOnWallTime > 0 && LastOnGroundTime <= 0 && (!IsWallJumping ||
			 (LastOnWallRightTime > 0 && _lastWallJumpDir == 1) || (LastOnWallLeftTime > 0 && _lastWallJumpDir == -1)) && Time.timeScale != 0f;
	}

	private bool CanJumpCut()
    {
		return IsJumping && RB.velocity.y > 0 && Time.timeScale != 0f;
    }

	private bool CanWallJumpCut()
	{
		return IsWallJumping && RB.velocity.y > 0 && Time.timeScale != 0f;
	}

	private bool CanRoll()
	{
		if (!IsRolling && _rollsLeft < Data.rollAmount && LastOnGroundTime > 0 && !_rollRefilling)
		{
			StartCoroutine(nameof(RefillRoll), 1);
		}

		return _rollsLeft > 0 && Time.timeScale != 0f;
	}

	private bool CanCrouch()
	{
		return LastOnGroundTime > 0 && Time.timeScale != 0f;
	}
    #endregion


    #region EDITOR METHODS
    private void OnDrawGizmosSelected()
    {
		Gizmos.color = Color.green;
		Gizmos.DrawWireCube(_groundCheckPoint.position, _groundCheckSize);
		Gizmos.color = Color.blue;
		Gizmos.DrawWireCube(_frontWallCheckPoint.position, _wallCheckSize);
		Gizmos.DrawWireCube(_backWallCheckPoint.position, _wallCheckSize);
	}
    #endregion
}