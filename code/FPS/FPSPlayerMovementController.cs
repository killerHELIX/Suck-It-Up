using Sandbox;
using Sandbox.Citizen;

public sealed class FPSPlayerMovementController : Component
{

	[Property] public float GroundControl { get; set; } = 4.0f;
	[Property] public float AirControl { get; set; } = 0.1f;
	[Property] public float MaxForce { get; set; } = 50f;
	[Property] public float Speed { get; set; } = 160f;
	[Property] public float RunSpeed { get; set; } = 290f;
	[Property] public float CrouchSpeed { get; set; } = 90f;
	[Property] public float JumpForce { get; set; } = 400f;

	[Property] public GameObject Head { get; set; }
	[Property] public GameObject Body { get; set; }

	public Vector3 WishVelocity = Vector3.Zero;
	[Sync] public bool IsCrouching { get; set; } = false;
	[Sync] public bool IsSprinting { get; set; } = false;
	[Sync] Angles TargetAngle { get; set; }

	private CharacterController CharacterController;
	private CitizenAnimationHelper AnimationHelper;
	private ModelRenderer BodyRenderer;

	protected override void OnAwake()
	{
		if (IsProxy) return;

		CharacterController = Components.Get<CharacterController>();
		AnimationHelper = Components.Get<CitizenAnimationHelper>();
		Head = GameObject.Children.FirstOrDefault(x => x.Tags.Contains("playerhead"));
		Body = GameObject.Children.FirstOrDefault(x => x.Tags.Contains("playerbody"));
		BodyRenderer = Body.Components.Get<ModelRenderer>();
		Log.Info($"{this} Head: {Head} Body: {Body}");

		// if (!IsProxy) Network.Refresh();

	}

	protected override void OnUpdate()
	{
		// return; 
		if (!IsProxy)
		{
			// Set our sprinting and crouching states
			UpdateCrouch();
			IsSprinting = Input.Down("Run");

			if (Input.Pressed("Jump")) Jump();

			TargetAngle = new Angles(0, Head.Transform.Rotation.Yaw(), 0).ToRotation();

			RotateBody();
			UpdateAnimations();
		}


	}

	protected override void OnFixedUpdate()
	{
		if (IsProxy) return;

		BuildWishVelocity();
		Move();
	}

	void BuildWishVelocity()
	{
		WishVelocity = 0;

		var rot = Head.Transform.Rotation;

		if (Input.Down("Forward")) WishVelocity += rot.Forward;
		if (Input.Down("Backward")) WishVelocity += rot.Backward;
		if (Input.Down("Left")) WishVelocity += rot.Left;
		if (Input.Down("Right")) WishVelocity += rot.Right;

		WishVelocity = WishVelocity.WithZ(0);

		if (!WishVelocity.IsNearZeroLength) WishVelocity = WishVelocity.Normal;

		if (IsCrouching) WishVelocity *= CrouchSpeed;
		else if (IsSprinting) WishVelocity *= RunSpeed;
		else WishVelocity *= Speed;
	}

	void Move()
	{
		// Get gravity from scene
		var gravity = Scene.PhysicsWorld.Gravity;

		if (CharacterController.IsOnGround)
		{
			// Apply friction/accel
			CharacterController.Velocity = CharacterController.Velocity.WithZ(0);
			CharacterController.Accelerate(WishVelocity);
			CharacterController.ApplyFriction(GroundControl);
		}
		else
		{
			CharacterController.Velocity += gravity * Time.Delta * 0.5f;

			CharacterController.Accelerate(WishVelocity.ClampLength(MaxForce));
			CharacterController.ApplyFriction(AirControl);
		}


		// Move the character controller
		CharacterController.Move();

		// Apply the second half of gravity after movement
		if (!CharacterController.IsOnGround)
		{
			CharacterController.Velocity += gravity * Time.Delta * 0.5f;
		}
		else
		{
			CharacterController.Velocity = CharacterController.Velocity.WithZ(0);
		}

	}

	void RotateBody()
	{
		if (Body is null) return;

		float rotateDifference = Body.Transform.Rotation.Distance(TargetAngle);

		if (rotateDifference > 50f || CharacterController.Velocity.Length > 10f)
		{
			Body.Transform.Rotation = Rotation.Lerp(Body.Transform.Rotation, TargetAngle, Time.Delta * 2f);
		}
	}


	void Jump()
	{

		if (!CharacterController.IsOnGround) return;

		CharacterController.Punch(Vector3.Up * JumpForce);
		AnimationHelper?.TriggerJump();

	}

	void UpdateAnimations()
	{
		// BodyRenderer.RenderType = (Network.IsProxy) ? ModelRenderer.ShadowRenderType.On;

		if (AnimationHelper is null) return;
		// Log.Info(AnimationHelper.Target);
		AnimationHelper.WithWishVelocity(WishVelocity);
		AnimationHelper.WithVelocity(CharacterController.Velocity);
		AnimationHelper.AimAngle = TargetAngle;
		AnimationHelper.IsGrounded = CharacterController.IsOnGround;
		AnimationHelper.WithLook(TargetAngle.Forward, 1f, 0.75f, 0.5f);
		AnimationHelper.MoveStyle = CitizenAnimationHelper.MoveStyles.Run;
		AnimationHelper.DuckLevel = IsCrouching ? 1f : 0f;

	}

	void UpdateCrouch()
	{
		if (CharacterController is null) return;

		if (Input.Pressed("crouch") && !IsCrouching)
		{
			IsCrouching = true;
			CharacterController.Height /= 2f;

		}

		if (Input.Released("crouch") && IsCrouching)
		{
			var trace = Scene.Trace.Ray(Transform.Position, Transform.Rotation.Up * (CharacterController.Height * 2f))
				.WithoutTags("player", "trigger")
				.Run();

			if (!trace.Hit)
			{
				IsCrouching = false;
				CharacterController.Height *= 2f;
			}
		}
	}

}
