public sealed class FPSCameraController : Component
{

	[Property] public float Distance { get; set; } = 0f;
	[Property] public float ThirdPersonHorizontalDistance { get; set; } = 30f;
	[Property] public CameraComponent Camera {  get; set; }

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
		CurrentOffset = Vector3.Lerp(CurrentOffset, targetOffset, Time.Delta * 10f);

		if (Camera != null && BodyRenderer != null)
		{
			var camPos = PlayerHeadRef.Transform.Position + CurrentOffset;

			// Set position of the camera to the calculated position
			Camera.Transform.Position = camPos;
			Camera.Transform.Rotation = Rotation.From(eyeAngles);
		}
	}
}