
public sealed class FPSWeaponController : Component
{
	[Property] public CameraComponent Head { get; set; }
	private SceneTraceResult lastTraceResult;

	protected override void OnUpdate()
	{
		if ( !IsProxy )
		{
			WeaponInput();

		// Debug draw the last projectile fired.
		Gizmo.Draw.SolidSphere(lastTraceResult.EndPosition, 2.0f);
		}
	}

	private void WeaponInput()
	{
		if ( Input.Pressed( "select" ))
		{

			Log.Info( "Fire" );
			float dist = 10000.0f;
			lastTraceResult = Scene.Trace.Ray( new Ray(Head.Transform.Position, Head.Transform.Rotation.Forward * dist), dist ).Run();

			if ( lastTraceResult.Hit )
			{
				Log.Info( $"Hit: {lastTraceResult.GameObject} at {lastTraceResult.EndPosition}" );
			}
		}

		if ( Input.Down( "Command" ))
		{
			Log.Info( "Aiming down sights" );
			Head.FieldOfView = 60f;
		} else {
			Head.FieldOfView = 90f;
		}

	}
}