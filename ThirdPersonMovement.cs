using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class ThirdPersonMovement : MonoBehaviour
{
	public CharacterController controller;

	public Transform cam;

	public float speed = 6f;
	public float sprintSpeed;
	public bool sprintToggle = false;
	public float turnSmoothTime = 0.1f;
	public float turnSmoothVelocity;

	public Vector3 velocity;
	public float gravity = -20f;

	public Transform groundCheck;
	public float groundDistance = 0.4f;
	public LayerMask groundMask;

	bool isGrounded;
	public float jumpHieght = 3f;

	private bool _sprinting;
	private float _speed;
	
	private Vector3 _dirVec = Vector3.zero;
	
	private void Start()
	{
		_speed = speed;
	}

	// Update is called once per frame
	void Update()
	{
		if (PlayerManager.Instance.AcceptPlayerInput) {
			isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
			//print(isGrounded);

			if (isGrounded && velocity.y < 0){
				velocity.y = -2f;
			}

			velocity.y += gravity * Time.deltaTime;
			controller.Move(velocity * Time.deltaTime);
			
			// Only continue if there's a movement vector to operate on
			if (_dirVec == Vector3.zero) return;
			
			float targetAngle = Mathf.Atan2(_dirVec.x, _dirVec.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
			float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
			transform.rotation = Quaternion.Euler(0f, angle, 0f);

			Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
			controller.Move(moveDir.normalized*_speed*Time.deltaTime);
		} else if (PlayerManager.Instance.Coords.Count != 0) {
			controller.Move(PlayerManager.Instance.GetNextCoord());
			//Debug.Log("Target Coords: " + PlayerManager.Instance.GetNextCoord());
			PlayerManager.Instance.PopCoord();
		} else {
			PlayerManager.Instance.SetPlayerInputActive();	
		}
	}

	/// <summary>
	/// Input action callback for handling movement. Sets the private _dirVec to the latest input vector
	/// </summary>
	/// <param name="context">Context for callback. This has the input vector2</param>
	public void HandleMoveInput(InputAction.CallbackContext context)
	{
		Vector2 inputVec = context.ReadValue<Vector2>();
		_dirVec = new Vector3(inputVec.x, 0f, inputVec.y);
	}

	/// <summary>
	/// Input action callback for handling jump. Only does so on initial press
	/// </summary>
	/// <param name="context">Context for callback</param>
	public void HandleJump(InputAction.CallbackContext context)
	{
		// Only trigger a jump if it's when it was first pressed
		if (!context.started) return;
		if (isGrounded) velocity.y = Mathf.Sqrt(jumpHieght * -2f * gravity);
	}

	/// <summary>
	/// Handle sprint according to whether sprint toggle is on or not
	/// </summary>
	/// <param name="context">Context for callback</param>
	public void Sprint(InputAction.CallbackContext context)
	{
		// On press
		if (context.started)
		{
			if (sprintToggle)
			{
				// If toggle, change
				_speed = _sprinting ? sprintSpeed : speed;
				_sprinting = !_sprinting;
			}
			else
			{
				// Not toggle, so set sprint speed
				_speed = sprintSpeed;
			}
		}
		// Otherwise if button was released and it isn't toggle, revert sprint speed
		else if (context.canceled && !sprintToggle) _speed = speed;
	}
}
