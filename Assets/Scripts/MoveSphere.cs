using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveSphere : MonoBehaviour
{
	[SerializeField, Range(0f, 100f)] float maxSpeed = 10f;

	[SerializeField, Range(0f, 100f)] float maxAcceleration = 10f, maxAirAcceleration = 1f;

	[SerializeField, Range(0f, 10f)] float jumpHeight = 2f;

	[SerializeField, Range(0, 5)] int maxAirJumps = 0;

	[SerializeField, Range(0f, 90f)] float maxGroundAngle = 25f;

	Vector3 velocity, desiredVelocity;

	int jumpPhase;

	Rigidbody body;

	bool desiredJump,onGround;

	float minGroundDotProduct;

	Vector3 contactNormal;

	void OnValidate()
	{
		minGroundDotProduct = Mathf.Cos(maxGroundAngle*Mathf.Deg2Rad);
	}

	void Awake()
	{
		OnValidate();
		body = GetComponent<Rigidbody>();
	}

	void OnCollisionEnter(Collision collision)
	{
		EvaluateCollision(collision);
	}

	void OnCollisionStay(Collision collision)
	{
		EvaluateCollision(collision);
	}

	void Update()
	{

		Vector2 playerInput;

		velocity = body.velocity;

		playerInput.x = Input.GetAxis("Horizontal");
		playerInput.y = Input.GetAxis("Vertical");
		playerInput = Vector2.ClampMagnitude(playerInput, 1f);

		desiredVelocity = new Vector3(playerInput.x, 0f, playerInput.y) * maxSpeed;

		desiredJump |= Input.GetButtonDown("Jump");

	}

    private void FixedUpdate()
    {
		float acceleration = onGround ? maxAcceleration : maxAirAcceleration;
		float maxSpeedChange = acceleration * Time.deltaTime;

		velocity.x = Mathf.MoveTowards(velocity.x, desiredVelocity.x, maxSpeedChange);
		velocity.z = Mathf.MoveTowards(velocity.z, desiredVelocity.z, maxSpeedChange);

		if (desiredJump)
		{
			desiredJump = false;
			Jump();
		}

		UpdateState();

		onGround = false;

	}

	void EvaluateCollision(Collision collision) 
	{
		for (int i = 0; i < collision.contactCount; i++)
		{
			Vector3 normal = collision.GetContact(i).normal;
			if (normal.y >= minGroundDotProduct)
			{
				onGround = true;
				contactNormal = normal;
			}
		}
	}
	private void Jump()
    {
		if (onGround || jumpPhase < maxAirJumps)
		{
			jumpPhase += 1;
			float jumpSpeed = Mathf.Sqrt(-2f * Physics.gravity.y * jumpHeight);
			float alignedSpeed = Vector3.Dot(velocity, contactNormal);
			if (alignedSpeed > 0f)
			{
				jumpSpeed = Mathf.Max(jumpSpeed - alignedSpeed, 0f);
			}
			else
			{
				contactNormal = Vector3.up;
			}
			velocity.y += jumpSpeed;
		}
	}

	void UpdateState()
	{
		body.velocity=velocity;
		if (onGround)
		{
			jumpPhase = 0;
		}
	}


}