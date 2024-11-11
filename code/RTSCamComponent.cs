
public class RTSCamComponent : Component
{
	[Property] public CameraComponent CamView { get; set; }
	[Property] public float CamDefaultSpeed {  get; set; }
	[Property] public float CamFastSpeedMultiplier { get; set; }
	[Property] public float CamZoomSpeed { get; set; }
	[Property] public float CamRotateSpeed { get; set; }

	float currentCamHeight = 400.0F;
	private const float trackHeightMultiplier = 10f;
	private const float trackSpeedMultiplier = .25f;
	private const float trackhorizontalOffset = 1000f;

	float camYaw;
	float camPitch;
	CameraMode camMode = CameraMode.Ortho;
	public GameObject trackTarget;

	public enum CameraMode
	{
		Ortho,
		Tracking
	}

	protected override void OnStart()
	{
		if (Network.IsProxy)
		{
			CamView.IsMainCamera = false;
			CamView.Enabled = false;
			Enabled = false;
		}
		else
		{
			CamView.IsMainCamera = true;
		}
		base.OnStart();
	}

	protected override void OnUpdate()
	{
		if (Network.IsProxy) {
			return; 
		}

		if(Input.Released("Change Camera Mode"))
		{
			if(camMode == CameraMode.Ortho)
			{
				trackTarget = RTSPlayer.Local.UnitControl.SelectedObjects.First<SelectableObject>().GameObject;//GameState.Local.
				camMode = CameraMode.Tracking;
				Log.Info("Cam mode is now Tracking");
			}
            else
            {
				camMode = CameraMode.Ortho;
				Log.Info("Cam mode is now Ortho");
			}
        }

		if(camMode == CameraMode.Ortho)
		{
			//Downwards ray calculates global z-level of the ground
			var groundRay = Scene.Trace.Ray(new Ray(Transform.Position, Vector3.Down), 5000.0F);
			var tr = groundRay.Run();
			var currentGroundZLevel = tr.EndPosition.z;
			//Log.Info("Z Level Below Me: " +  currentGroundZLevel);

			// Move Camera
			Vector3 movement = 0;

			// Multiply speed if necessary
			var CamSpeed = CamDefaultSpeed;
			if (Input.Down("Run"))
			{
				CamSpeed = CamDefaultSpeed * CamFastSpeedMultiplier;
			}

			// Handle WASD Movement
			if (Input.Down("Forward"))
			{
				movement += Transform.World.Forward.WithZ(0) * CamSpeed;
			}
			if (Input.Down("Backward"))
			{
				movement += Transform.World.Backward.WithZ(0) * CamSpeed;
			}
			if (Input.Down("Left"))
			{
				movement += Transform.World.Left.WithZ(0) * CamSpeed;
			}
			if (Input.Down("Right"))
			{
				movement += Transform.World.Right.WithZ(0) * CamSpeed;
			}
			// Handle Zoom Level
			if (Input.MouseWheel.x < 0 || Input.MouseWheel.y < 0)
			{
				movement.z = CamZoomSpeed;
			}
			if (Input.MouseWheel.x > 0 || Input.MouseWheel.y > 0)
			{
				movement.z = -CamZoomSpeed;
			}
			// Cam Height must mandatorily stay above the floor by some amount
			//if(currentCamHeight < 100) currentCamHeight = 100;
			// If we fly above a gigantic pitt or off the map, don't launch us to hell
			/*if (float.Abs( tr.EndPosition.z - Transform.Position.z) >  4000.0)
			{
				movement.z = 0;
			}
			else
			{
				// Calculated z value
				movement.z = ((tr.EndPosition.z + currentCamHeight) - Transform.Position.z);
			}*/
			//movement.z = Transform.Position.z;

			// Rotate Camera
			camYaw = Transform.Rotation.Yaw();
			camPitch = Transform.Rotation.Pitch();

			// Add to current position
			var rot = GameObject.Transform.Rotation;
			var pos = GameObject.Transform.Position + new Vector3(movement.x, movement.y, -movement.z);

			//Handle Horizontal Rotation
			if (Input.Down("Rotate Left"))
			{
				camYaw += Time.Delta * CamRotateSpeed;
			}
			if (Input.Down("Rotate Right"))
			{
				camYaw -= Time.Delta * CamRotateSpeed;
			}

			//Log.Info( "Rotation After Horizontal Rotate" + rot.Angles() );

			// Handle Pitch Rotation
			if (Input.Down("Pitch Up"))
			{
				camPitch += Time.Delta * CamRotateSpeed;
			}
			if (Input.Down("Pitch Down"))
			{
				camPitch -= Time.Delta * CamRotateSpeed;
			}
			camPitch = camPitch.Clamp(0, 89.9f);

			// Create Quat Rotation
			rot = Rotation.From(camPitch, camYaw, 0);

			// Set Transform
			Transform.LerpTo(new Transform(pos, rot, 1), 0.1f);
		}

		if(camMode == CameraMode.Tracking)
		{
			// Handle Zoom Level
			if (Input.MouseWheel.x < 0 || Input.MouseWheel.y < 0)
			{
				currentCamHeight = currentCamHeight + CamZoomSpeed * trackSpeedMultiplier;
			}
			if (Input.MouseWheel.x > 0 || Input.MouseWheel.y > 0)
			{
				currentCamHeight = currentCamHeight - CamZoomSpeed * trackSpeedMultiplier;
			}

			//Handle Horizontal Rotation
			if (Input.Down("Rotate Left"))
			{
				camYaw -= Time.Delta * CamRotateSpeed * trackSpeedMultiplier;
			}
			if (Input.Down("Rotate Right"))
			{
				camYaw += Time.Delta * CamRotateSpeed * trackSpeedMultiplier;
			}

			var newrot = Rotation.LookAt(Vector3.Direction(WorldPosition, trackTarget.WorldPosition));
			var offsetPos = new Vector3(trackTarget.WorldPosition.x + trackhorizontalOffset, trackTarget.WorldPosition.y, trackTarget.WorldPosition.z);
			var newPosFlat = offsetPos.RotateAround(trackTarget.WorldPosition, Rotation.FromYaw(camYaw));
			var newPos = new Vector3(newPosFlat.x, newPosFlat.y, newPosFlat.z + currentCamHeight);
			//Log.Info("track position: " + trackTarget.WorldPosition.x + ", " + trackTarget.WorldPosition.y + ", " + trackTarget.WorldPosition.z);
			//Log.Info("track position offset: " + newPos.x + ", " + newPos.y + ", " + newPos.z);
			//Log.Info("track rotation angles: " + newrot.Pitch() + ", " + newrot.Yaw() + ", " + newrot.Roll());
			Transform.LerpTo(new Transform(newPos, newrot, 1), 0.25f);
		}
	}
}
