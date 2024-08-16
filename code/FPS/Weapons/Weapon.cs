using Sandbox;
public abstract class Weapon : Component, Component.ICollisionListener
{

	public abstract float GetViewmodelXOffset();
	public abstract float GetViewmodelYOffset();
	public abstract float GetViewmodelZOffset();
	public abstract void Fire();

	public void OnCollisionStart(Collision collision)
	{
		var selfObj = collision.Self.Collider.GameObject;
		var otherObj = collision.Other.Collider.GameObject;

		Log.Info($"Collision: {collision.Self.Collider.GameObject} {collision.Other.Collider.GameObject}");

		var playerWeaponController = otherObj.Components.Get<FPSWeaponController>();
		if (playerWeaponController != null)
		{
			Log.Info("Collided with player");
			GameObject.Parent = playerWeaponController.GameObject;
			Components.Get<Rigidbody>().Enabled = false;
			GameObject.Tags.Add("playerweapon");
			playerWeaponController.Weapons.Add(this);
		}
		
	}

	public void OnCollisionUpdate(Collision collision)
	{
	}
	public void OnCollisionStop(CollisionStop collision)
	{
	}

}
