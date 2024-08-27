using System;

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
	private Angles EyeAngles = Angles.Zero;
	private float NoRecoil = 100f;
	private float MaxRecoil = 30f;
	private float RecoilRecovery = 100f;
	private float CurrentRecoil;

	public bool IsRecoiling {
		get {
			return CurrentRecoil < NoRecoil;
		}
	}


	protected override void OnStart()
	{
		CurrentRecoil = NoRecoil;

		if (IsProxy) 
		{
			Camera.Enabled = false;
			Camera.IsMainCamera = false;
			Enabled = false;
		}
	}


	protected override void OnUpdate()
	{
		// Recoil feeds the final Rotation Slerp and goes from a low number (slow lerp) to a high number (fast lerp). 
		// Time.Delta naturally pushes the cam rotation toward fast lerp, AKA responsive cam.
		// This makes recoil smoother than pitching up instantly.
		CurrentRecoil = CurrentRecoil + Time.Delta * RecoilRecovery;
		CurrentRecoil = CurrentRecoil.Clamp(MaxRecoil, NoRecoil);

		if (IsProxy) return; 
		if (Player is null)
		{
			Player = Game.ActiveScene.GetAllComponents<FPSPlayerMovementController>().FirstOrDefault(x => x.Network.IsOwner);
			PlayerHeadRef = Player.HeadObject;
			PlayerBodyRef = Player.BodyObject;
			BodyRenderer = PlayerBodyRef.Components.Get<ModelRenderer>();
		}


		if (EyeAngles == Angles.Zero) EyeAngles = PlayerHeadRef.Transform.Rotation.Angles();

		EyeAngles.pitch += Input.MouseDelta.y * 0.1f;
		EyeAngles.yaw -= Input.MouseDelta.x * 0.1f;
		EyeAngles.roll = 0f;
		EyeAngles.pitch = EyeAngles.pitch.Clamp(-89.9f, 89.9f);
		PlayerHeadRef.Transform.Rotation = Rotation.From(EyeAngles);

		// Set the current camera offset
		var targetOffset = Vector3.Zero;
		if (Player.IsCrouching) targetOffset += Vector3.Down * 32f;
		CurrentOffset = Vector3.Lerp(CurrentOffset, targetOffset, Time.Delta * 10f);

		if (Camera != null && BodyRenderer != null)
		{
			var camPos = PlayerHeadRef.Transform.Position + CurrentOffset;

			// Set position of the camera to the calculated position
			Camera.Transform.Position = camPos;
			Camera.Transform.Rotation = Rotation.Slerp(Camera.Transform.Rotation, Rotation.From(EyeAngles), Time.Delta * CurrentRecoil);
		}
	}

    public void Recoil(float strength)
    {
		if (IsProxy) return;
		EyeAngles = PlayerHeadRef.Transform.Rotation.Angles();
		EyeAngles.pitch -= strength;

		CurrentRecoil = MaxRecoil;
    }

}