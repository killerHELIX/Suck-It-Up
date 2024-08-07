
public sealed class FPSWeaponController : Component
{
	[Property] public CameraComponent Head { get; set; }

	[Property] public List<Pistol> Weapons { get; set; }

	[Property] public float WeaponX { get; set; } = 20f;
	[Property] public float WeaponY { get; set; } = 8f;
	[Property] public float WeaponZ { get; set; } = -6f;

	private SceneTraceResult lastTraceResult;

	protected override void OnStart()
	{
		if ( !IsProxy )
		{
			// pass
		}
	}

	protected override void OnUpdate()
	{
		if ( !IsProxy )
		{
			WeaponInput();

			var weapon = Weapons[0];
			weapon.Transform.Position = Head.Transform.Position + (Head.Transform.Rotation.Forward * WeaponX) + (Head.Transform.Rotation.Right * WeaponY) + (Head.Transform.Rotation.Up * WeaponZ);
			weapon.Transform.Rotation = Head.Transform.Rotation;

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