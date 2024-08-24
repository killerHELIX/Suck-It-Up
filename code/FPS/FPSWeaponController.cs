using System;

public sealed class FPSWeaponController : Component
{

	[Property] public GameObject Head;
	[Property] public GameObject Body;
	[Sync] [Property] public List<Weapon> Weapons { get; set; }


	[Sync] [Property] public Weapon SelectedWeapon { get; set; }


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
			var wep = Weapons.ElementAtOrDefault(0);
			if (SelectedWeapon != null && SelectedWeapon != wep) SelectedWeapon.Holster();

			SelectedWeapon = wep;
		}

		if (Input.Pressed("slot2"))
		{
			var wep = Weapons.ElementAtOrDefault(1);
			if (SelectedWeapon != null && SelectedWeapon != wep) SelectedWeapon.Holster();
			SelectedWeapon = wep;
		}

		if (Input.Pressed("slot3"))
		{
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
			if (Input.Pressed("reload")) SelectedWeapon.Reload();

			SelectedWeapon.Aim();
		}

	}
}