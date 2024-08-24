public sealed class FPSCameraController : Component
{

	[Property] public float Distance { get; set; } = 40f;
	[Property] public float ThirdPersonHorizontalDistance { get; set; } = 30f;
	[Property] public CameraComponent Camera {  get; set; }

	public bool IsFirstPerson => Distance <= 0f;
	private Vector3 CurrentOffset = Vector3.Zero;
	private ModelRenderer BodyRenderer;
	private FPSPlayerMovementController Player { get; set; }
	private GameObject PlayerBodyRef { get; set; }
	private GameObject PlayerHeadRef { get; set; }


	/*protected override void OnAwake()
	{
		Camera = Components.Get<CameraComponent>();
	}*/


	protected override void OnUpdate()
	{
		if (IsProxy) return; 
		if (Player is null)
		{
			Player = Game.ActiveScene.GetAllComponents<FPSPlayerMovementController>().FirstOrDefault(x => x.Network.IsOwner);
			PlayerHeadRef = Player.HeadObject;
			PlayerBodyRef = Player.BodyObject;
			BodyRenderer = PlayerBodyRef.Components.Get<ModelRenderer>();
		}


		var eyeAngles = PlayerHeadRef.Transform.Rotation.Angles();
		eyeAngles.pitch += Input.MouseDelta.y * 0.1f;
		eyeAngles.yaw -= Input.MouseDelta.x * 0.1f;
		eyeAngles.roll = 0f;
		eyeAngles.pitch = eyeAngles.pitch.Clamp(-89.9f, 89.9f);
		PlayerHeadRef.Transform.Rotation = Rotation.From(eyeAngles);

		// Set the current camera offset
		var targetOffset = Vector3.Zero;
		if (Player.IsCrouching) targetOffset += Vector3.Down * 32f;
		targetOffset += IsFirstPerson ? Vector3.Zero : PlayerHeadRef.Transform.Rotation.Right.Normal * ThirdPersonHorizontalDistance;
		CurrentOffset = Vector3.Lerp(CurrentOffset, targetOffset, Time.Delta * 10f);

		if (Camera != null && BodyRenderer != null)
		{
			var camPos = PlayerHeadRef.Transform.Position + CurrentOffset;
			if (!IsFirstPerson)
			{
				// Perform a trace backwards to see where we can safely place the cam
				var camForward = eyeAngles.ToRotation().Forward;
				var camTrace = Scene.Trace.Ray(camPos, camPos - (camForward * Distance))
					.WithoutTags("player", "trigger")
					.Run();

				if (camTrace.Hit)
				{
					camPos = camTrace.HitPosition + camTrace.Normal;
				}
				else
				{
					camPos = camTrace.EndPosition;
				}

				// Show the body if we're not in the first person
				BodyRenderer.RenderType = ModelRenderer.ShadowRenderType.On;
			}
			else
			{
				// Show the body if we're not in the first person
				BodyRenderer.RenderType = ModelRenderer.ShadowRenderType.ShadowsOnly;

			}

			// Set position of the camera to the calculated position
			Camera.Transform.Position = camPos;
			Camera.Transform.Rotation = Rotation.From(eyeAngles);
		}
	}
}