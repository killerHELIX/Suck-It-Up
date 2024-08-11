
using System.Numerics;

public sealed class FPSWeaponController : Component
{
	[Property] public CameraComponent Head { get; set; }

	[Property] public List<Pistol> Weapons { get; set; }

	[Property] public float WeaponX { get; set; } = 25f;
	[Property] public float WeaponY { get; set; } = 8f;
	[Property] public float WeaponZ { get; set; } = -10f;
	[Property] public float WeaponSwaySpeed { get; set; } = 50f;


	protected override void OnStart()
	{
		if ( !IsProxy )
		{
			// pass
		}

		Log.Info("All IWeapon Components in Scene:");
		var weapons = Scene.GetAllComponents<IWeapon>();
		foreach (IWeapon weapon in weapons)
		{
			Log.Info($"Simulating pickup of weapon: {weapon}");
			Weapons.Add(weapon.GameObject.Clone(weapon.Transform.Position + Vector3.Forward * 10).Components.Get<Pistol>());
			break;
		}
	}

	protected override void OnUpdate()
	{
		if ( !IsProxy )
		{
			WeaponInput();

			var weapon = Weapons[0];

			var targetPos = Head.Transform.Position
				+ (Head.Transform.Rotation.Forward * WeaponX) 
				+ (Head.Transform.Rotation.Right * WeaponY) 
				+ (Head.Transform.Rotation.Up * WeaponZ);


			weapon.Transform.Position = Vector3.Lerp(weapon.Transform.Position, targetPos, Time.Delta * WeaponSwaySpeed);
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