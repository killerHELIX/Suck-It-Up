
public sealed class FPSWeaponController : Component
{
	[Property] public CameraComponent Head { get; set; }

	[Property] public List<Pistol> Weapons { get; set; }

	[Property] public float WeaponX { get; set; } = 20f;
	[Property] public float WeaponY { get; set; } = 8f;
	[Property] public float WeaponZ { get; set; } = -6f;


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

		}
	}

	private void WeaponInput()
	{
		var weapon = Weapons[0];

		if ( Input.Pressed( "select" )) weapon.Fire();
		if ( Input.Pressed( "reload" )) weapon.Reload();
	}
}