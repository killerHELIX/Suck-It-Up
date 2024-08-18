using System;

public sealed class FPSWeaponController : Component
{
	[Property] public CameraComponent Head { get; set; }

	[Property] public List<Weapon> Weapons { get; set; }

	private Weapon SelectedWeapon;


	protected override void OnStart()
	{
		if (!IsProxy)
		{
			// pass
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
		if (Input.Pressed("slot1"))
		{
			Log.Info("Equipping weapon slot 1");
			var wep = Weapons.ElementAtOrDefault(0);
			if (SelectedWeapon != null && SelectedWeapon != wep) SelectedWeapon.Holster();

			SelectedWeapon = wep;
		}

		if (Input.Pressed("slot2"))
		{
			Log.Info("Equipping weapon slot 2");
			var wep = Weapons.ElementAtOrDefault(1);
			if (SelectedWeapon != null && SelectedWeapon != wep) SelectedWeapon.Holster();
			SelectedWeapon = wep;
		}

		if (Input.Pressed("slot3"))
		{
			Log.Info("Holstering selected weapon");
			if (SelectedWeapon != null) SelectedWeapon.Holster();
			SelectedWeapon = null;
		}

		if (SelectedWeapon != null)
		{
			if (Input.Pressed("drop"))
			{
				Weapons.Remove(SelectedWeapon);
				SelectedWeapon.Drop();
				SelectedWeapon = null;
				return;
			}
			if (Input.Down("select")) SelectedWeapon.Fire(); // Fire the weapon as long as the input is held. The weapon knows its own rate of fire.

			if (SelectedWeapon.GetType().IsSubclassOf(typeof(Gun)))
			{
				Gun gun = (Gun)SelectedWeapon;
				if (Input.Pressed("reload")) gun.Reload();
			}

			SelectedWeapon.Aim();
		}

	}
}