using Sandbox;
public abstract class Weapon : Component, Component.ICollisionListener
{

    [Property] 
	[Description("The position of this weapon's viewmodel when equipped.")]
	public abstract Vector3 ViewmodelPosition { get; set; }

    [Property] 
	[Description("The position of this weapon's model when holstered.")]
    public abstract Vector3 HolsterPosition { get; set; }

    [Property] 
	[Description("The rotation of this weapon's model when holstered.")]
    public abstract Rotation HolsterRotation { get; set; }

    [Property] 
	[Description("This distance this weapon should start away from the player when dropped.")]
	public abstract float DropDistance { get; set; }

    [Property] 
	[Description("The speed this weapon should go up when dropped.")]
	public abstract float DropSpeedUp { get; set; }

    [Property] 
	[Description("The speed this weapon should go forward when dropped.")]
	public abstract float DropSpeedForward { get; set; }

	public abstract void Fire();

	public void Pickup(GameObject player, FPSWeaponController playerWeaponController)
	{
			Log.Info($"{player} picking up {this}");
			GameObject.SetParent(playerWeaponController.GameObject, keepWorldPosition: false);
			// Transform.Position = playerWeaponController.GameObject.Transform.Position;
			// Transform.LocalPosition = Vector3.Zero;
			// Transform.Rotation = Angles.Zero;
			Components.Get<Rigidbody>().Enabled = false;
			GameObject.Tags.Add("playerweapon");
			playerWeaponController.Weapons.Add(this);
			Holster();
	}

    public void Holster()
    {
		Transform.LocalPosition = HolsterPosition;
        // Transform.LocalPosition = Vector3.Lerp(Transform.LocalPosition, GetHolsterPosition(), Time.Delta * 2f);
		Transform.LocalRotation = HolsterRotation;
    }
    public void Aim()
    {
        var head = GameObject.Parent.Components.GetInChildrenOrSelf<CameraComponent>();
		// var viewmodelPos = GetViewmodelPosition();
        var targetPos = head.Transform.Position
            + (head.Transform.Rotation.Forward * ViewmodelPosition.x)
            + (head.Transform.Rotation.Right * ViewmodelPosition.y)
            + (head.Transform.Rotation.Up * ViewmodelPosition.z);

        Transform.Position = Vector3.Lerp(Transform.Position, targetPos, Time.Delta * 5f);
        Transform.Rotation = Rotation.Slerp(Transform.Rotation, head.Transform.Rotation, Time.Delta * 20f);
		
        // Transform.Position = targetPos;
        // Transform.Rotation = head.Transform.Rotation;
    }

    public void Drop()
	{

		Log.Info($"Dropping {this}");

		GameObject.SetParent(Scene);

		var forward = Transform.Rotation.Forward * DropDistance;
		Transform.Position += forward;

		GameObject.Tags.Remove("playerweapon");
		GameObject.Tags.Add("weapon");
		var rigidbody = Components.Get<Rigidbody>(includeDisabled: true);
		rigidbody.Enabled = true;

		var throwPosVec = (Transform.Rotation.Forward * DropSpeedForward) + (Transform.Rotation.Up * DropSpeedUp);
		rigidbody.ApplyImpulse(throwPosVec);

		// When dropping a weapon, randomly spin it left or right.
		var throwDir = (new System.Random().Next() % 2 == 0) ? Transform.Rotation.Left : Transform.Rotation.Right;
		var throwRotVec = (Transform.Rotation.Forward + throwDir) * 3f;
		rigidbody.AngularVelocity = throwRotVec;
	}

	public void OnCollisionStart(Collision collision)
	{
		var otherObj = collision.Other.Collider.GameObject;
		TryToPickup(otherObj);

	}

	public void OnCollisionUpdate(Collision collision)
	{
		// no op
	}

	public void OnCollisionStop(CollisionStop collision)
	{
		// no op
	}

	private void TryToPickup(GameObject other)
	{
		var playerWeaponController = other.Components.Get<FPSWeaponController>();
		if (playerWeaponController != null)
		{
			Pickup(other, playerWeaponController);
		}
	}

}
