public sealed class FPSWeaponController : Component
{
	[Property] public CameraComponent Head { get; set; }

	[Property] public List<Weapon> Weapons { get; set; }

	[Property] public float WeaponSwaySpeed { get; set; } = 50f;


	protected override void OnStart()
	{
		if (!IsProxy)
		{
			// pass
		}

		var weapons = Scene.GetAllComponents<Weapon>();
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

			if (weapon.GetType().IsSubclassOf(typeof(Gun)))
			{
				Gun gun = (Gun) weapon;
				if (Input.Pressed("reload")) gun.Reload();
			}

			var targetPos = Head.Transform.Position
				+ (Head.Transform.Rotation.Forward * weapon.GetViewmodelXOffset())
				+ (Head.Transform.Rotation.Right * weapon.GetViewmodelYOffset())
				+ (Head.Transform.Rotation.Up * weapon.GetViewmodelZOffset());


			weapon.Transform.Position = Vector3.Lerp(weapon.Transform.Position, targetPos, Time.Delta * WeaponSwaySpeed);
			weapon.Transform.Rotation = Head.Transform.Rotation;
		}
	}
}