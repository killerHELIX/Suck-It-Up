public sealed class FPSWeaponController : Component
{
	[Property] public CameraComponent Head { get; set; }

	[Property] public List<Weapon> Weapons { get; set; }

	[Property] public float WeaponX { get; set; } = 25f;
	[Property] public float WeaponY { get; set; } = 8f;
	[Property] public float WeaponZ { get; set; } = -10f;
	[Property] public float WeaponSwaySpeed { get; set; } = 50f;


	protected override void OnStart()
	{
		if (!IsProxy)
		{
			// pass
		}

		var weapons = Scene.GetAllComponents<Weapon>();
		Log.Info($"All Weapon Components in Scene: {weapons}");
		foreach (Weapon weapon in weapons)
		{
			if (weapon.Tags.Contains("weapon_template"))
			{
				Log.Info($"Simulating pickup of weapon: {weapon}");
				var clonedWep = weapon.GameObject.Clone(GameObject, Transform.Position, Transform.Rotation, Transform.Scale);
				clonedWep.Tags.Add("weapon");
				// Component clonedWepComponent;
				foreach (Component clonedWeaponComponent in clonedWep.Components.GetAll())
				{
					if (clonedWeaponComponent.GetType().IsSubclassOf(typeof(Weapon)))
					{
						Log.Info($"Found Weapon extender: {clonedWeaponComponent}");
						Weapons.Add((Weapon) clonedWeaponComponent);
					}

				}
				break;
			}
		}
	}

	protected override void OnUpdate()
	{
		if (!IsProxy)
		{
			WeaponInput();

		}
	}

	private void WeaponInput()
	{
		if (Weapons.Any())
		{

			var weapon = Weapons[0];

			if (Input.Pressed("select")) weapon.Fire();
			if (Input.Pressed("reload")) weapon.Reload();

			var targetPos = Head.Transform.Position
				+ (Head.Transform.Rotation.Forward * WeaponX)
				+ (Head.Transform.Rotation.Right * WeaponY)
				+ (Head.Transform.Rotation.Up * WeaponZ);


			weapon.Transform.Position = Vector3.Lerp(weapon.Transform.Position, targetPos, Time.Delta * WeaponSwaySpeed);
			weapon.Transform.Rotation = Head.Transform.Rotation;
		}
	}
}