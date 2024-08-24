using System;
using Sandbox;
public abstract class Weapon : Component, Component.ICollisionListener
{
    private const string PLAYER_OWNED_WEAPON = "playerweapon";
    private const string UNOWNED_WEAPON = "weapon";



    [Property] 
	[Description("The position of this weapon's viewmodel when equipped.")]
	public abstract String Name { get; set; }

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

    [Property] 
	[Description("The maximum amount of ammo in the weapon.")]
	public abstract int MaxAmmo { get; set; }

    [Property] 
	[Description("The maximum amount of ammo this weapon can hold in its reserve ammo pool.")]
	public abstract int MaxReserves { get; set; }

    [Property] 
	[Description("The rate of fire of this weapon in seconds.")]
	public abstract float FireRate { get; set; }

    [Property] 
	[Description("The reload speed of this weapon in seconds.")]
	public abstract float ReloadSpeed { get; set; }

    [Sync] 
    [Property]
	[Description("The current ammo in the weapon.")]
	public int CurrentAmmo { get; set; }

    [Sync] 
    [Property]
	[Description("The current reserve ammo pool of this weapon.")]
    public int CurrentReserves { get; set; }

    private float LastFireTime = 0f;
    private float LastReloadTime = 0f;
	private SceneTraceResult lastTraceResult;

    private FPSWeaponController PlayerWeaponController;



    public Weapon(){
        // Start gun loaded.
        CurrentAmmo = MaxAmmo;
        CurrentReserves = MaxReserves;
    }

	public void Pickup(GameObject player, FPSWeaponController playerWeaponController)
	{

        PlayerWeaponController = playerWeaponController;
        Log.Info($"{player} picking up {this}");
        GameObject.SetParent(PlayerWeaponController.Body, keepWorldPosition: false);
        Network.TakeOwnership();
        Components.Get<Rigidbody>().Enabled = false;
        GameObject.Tags.Add(PLAYER_OWNED_WEAPON);
        playerWeaponController.Weapons.Add(this);
        Holster();
	}

    public void Holster()
    {
		Transform.LocalPosition = HolsterPosition;
		Transform.LocalRotation = HolsterRotation;
    }
    public void Aim()
    {
        var head = PlayerWeaponController.Head;
        var targetPos = head.Transform.Position
            + (head.Transform.Rotation.Forward * ViewmodelPosition.x)
            + (head.Transform.Rotation.Right * ViewmodelPosition.y)
            + (head.Transform.Rotation.Up * ViewmodelPosition.z);

        Transform.Position = Vector3.Lerp(Transform.Position, targetPos, Time.Delta * 10f);
        Transform.Rotation = Rotation.Slerp(Transform.Rotation, head.Transform.Rotation, Time.Delta * 20f);
		
        // Transform.Position = targetPos;
        // Transform.Rotation = head.Transform.Rotation;
    }

    public void Drop()
	{

		Log.Info($"Dropping {this}");

		GameObject.SetParent(Scene);
        Network.DropOwnership();

		var forward = Transform.Rotation.Forward * DropDistance;
		Transform.Position += forward;

		GameObject.Tags.Remove(PLAYER_OWNED_WEAPON);
		GameObject.Tags.Add(UNOWNED_WEAPON);
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
	protected override void OnStart()
    {

    }

	protected override void OnUpdate()
	{
        // Log.Info($"{this} {Network.}");
		if ( !IsProxy )
		{

            // if (IsPaused())
            // {
            //     var fireTimeElapsed = System.Math.Round((decimal) (Time.Now - LastFireTime), 2);
            //     var reloadTimeElapsed = System.Math.Round((decimal) (Time.Now - LastReloadTime), 2);
            //     Log.Info($"Fire: [ {fireTimeElapsed}s // {FireRate}s ] Reload: [ {reloadTimeElapsed}s // {ReloadSpeed}s ]");
            // }
            // Debug draw the last projectile fired.
            Gizmo.Draw.SolidSphere(lastTraceResult.EndPosition, 2.0f);
		}
	}




    public virtual void Fire()
    {
        if (!IsPaused())
        {
            LastFireTime = Time.Now;

            if (CurrentAmmo <= 0)
            {
                Info("Empty!");
            }
            else
            {
                CurrentAmmo--;
                Info("Fired!");

                float dist = 10000.0f;
                var head = PlayerWeaponController.Head;

                // If the parent has a head shoot out of that. Otherwise shoot out of this gun directly.
                var origin = (head != null) ? head.Transform.Position : head.Transform.Position;
                lastTraceResult = Scene.Trace.Ray(new Ray(origin, Transform.Rotation.Forward * dist), dist).Run();

                if (lastTraceResult.Hit)
                {
                    Log.Info($"Hit: {lastTraceResult.GameObject} at {lastTraceResult.EndPosition}");
                }
            }
        }
    }

    // HL2 Style reloading. Pool of reserve bullets that refill a fixed magazine size. 
    public async void Reload()
    {
        if (!IsPaused())
        {
            if (CurrentAmmo == MaxAmmo)
            {
                Info("Already reloaded!");
            }
            else if (CurrentReserves <= 0)
            {
                Info("Out of reserves!");
            }
            else 
            {
                Info("Reloading...");
                LastReloadTime = Time.Now;
                await GameTask.Delay((ReloadSpeed * 1000f).CeilToInt()); // Wait for reload time (in ms) prior to actually updating ammo/reserves.

                var ammoDiff = MaxAmmo - CurrentAmmo;

                // If ammoDiff is less than reserves, reload full mag.
                if (ammoDiff <= CurrentReserves)
                {
                    CurrentReserves -= ammoDiff;
                    CurrentAmmo = MaxAmmo;
                }
                // Else, ammoDiff is greater than reserves, so load mag with whatever is left in reserves.
                else
                {
                    CurrentAmmo += CurrentReserves;
                    CurrentReserves -= CurrentReserves;
                }

                Info("Reloaded!");
            }
        }
    }



    protected void Info(string str)
    {
        Log.Info($"{str} [{CurrentAmmo} / {CurrentReserves} ]");
    }



    // The Gun is "Paused", i.e. ignoring player input, while certain actions are occurring like fire rate cooldown and reloading.
    private bool IsPaused()
    {
        bool firing = Time.Now - LastFireTime < FireRate;
        bool reloading = Time.Now - LastReloadTime < ReloadSpeed;
        return firing || reloading;
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
