﻿using System;
using Sandbox;
public abstract class Weapon : Component, Component.ICollisionListener
{
    private const string PLAYER_OWNED_WEAPON = "playerweapon";
    private const string UNOWNED_WEAPON = "weapon";
    private const int HEADSHOT_MULTIPLIER = 2;



    [Property]
    [Description("The viewmodel renderer of this weapon the player sees.")]
    public SkinnedModelRenderer ViewmodelRenderer { get; set; }

    [Property]
    [Description("The worldmodel renderer of this weapon the world sees.")]
    public SkinnedModelRenderer WorldmodelRenderer { get; set; }

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

	[Property]
	[Description("The health point damage this weapon does.")]
	public abstract int Damage { get; set; }

	[Property]
	[Description("The name of the sound event that should play when this weapon fires.")]
	public abstract string FireSoundEvent { get; set; }

	[Property]
	[Description("The strength of this weapon's recoil.")]
	public abstract float RecoilStrength { get; set; }

	[Sync]
    [Description("The current ammo in the weapon.")]
    public int CurrentAmmo { get; set; }

    [Sync]
    [Description("The current reserve ammo pool of this weapon.")]
    public int CurrentReserves { get; set; }

    private SceneTraceResult lastTraceResult;

    private FPSWeaponController PlayerWeaponController;
    private FPSCameraController PlayerCameraController;

    private bool ShouldDropOwnershipOnCollide = false;
    private bool isFiring = false;
	private bool isReloading = false;

    private bool animsInited = false;



	[Broadcast]
    public void Pickup(GameObject player, FPSWeaponController playerWeaponController, FPSCameraController playerCameraController)
    {

        PlayerWeaponController = playerWeaponController;
        PlayerCameraController = playerCameraController;

        Log.Info($"{player} picking up {this}");
        GameObject.SetParent(PlayerWeaponController.Body, keepWorldPosition: false);
        if (!player.IsProxy)
        {
            Network.TakeOwnership();
        }
        Components.Get<Rigidbody>().Enabled = false;
        GameObject.Tags.Add(PLAYER_OWNED_WEAPON);

        var existingWep = PlayerWeaponController.Weapons.FirstOrDefault(x => x.Name == Name);
        if (existingWep is not null)
        {
            existingWep.CurrentReserves += CurrentAmmo + CurrentReserves;
            existingWep.CurrentReserves = existingWep.CurrentReserves.Clamp(0, existingWep.MaxReserves);
            GameObject.Destroy();
        }
        else
        {
            playerWeaponController.Weapons.Add(this);
            Holster();
        }

        if(!player.IsProxy)
        {
            return;
        }

		if (!animsInited)
		{
			if (ViewmodelRenderer != null)
			{
                animsInited = true;
				ViewmodelRenderer.OnGenericEvent += HandleGenericAnimEvent;
				if (ViewmodelRenderer.SceneModel != null)
				{
					ViewmodelRenderer.SceneModel.SetAnimParameter("FireRate", FireRate);
				}
				else
				{
					Log.Info("No Scenemodel!");
				}
			}
			else
			{
				Log.Info("No viewmodel renderer!");
			}
		}

	}

    public void Holster()
    {
        isFiring = false;
        isReloading = false;
        ShowWorldmodel();

        Transform.LocalPosition = HolsterPosition;
        Transform.LocalRotation = HolsterRotation;
    }
    public void Aim()
    {
        if (!IsProxy) ShowViewmodel();

        var head = PlayerWeaponController.Head;
        var targetPos = head.Transform.Position
            + (head.Transform.Rotation.Forward * ViewmodelPosition.x)
            + (head.Transform.Rotation.Right * ViewmodelPosition.y)
            + (head.Transform.Rotation.Up * ViewmodelPosition.z);

        Transform.Position = Vector3.Lerp(Transform.Position, targetPos, Time.Delta * 10f);
        Transform.Rotation = Rotation.Slerp(Transform.Rotation, head.Transform.Rotation, Time.Delta * 20f);
    }

    [Broadcast]
    public void Drop()
    {
        Log.Info($"Dropping {this}");

        ShowWorldmodel();
        GameObject.SetParent(Scene);

        var dropDistance = PlayerWeaponController.Head.Transform.Rotation.Angles().pitch > 0f
            ? DropDistance * 1.5f
            : DropDistance;
        var forward = Transform.Rotation.Forward * dropDistance;
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

        if (!IsProxy)
        {
            ShouldDropOwnershipOnCollide = true; // Will drop ownership on next non-player collision
        }
    }

    public void OnCollisionStart(Collision collision)
    {
		if (collision.Other.Collider != null)
		{
			if (collision.Other.Collider.GameObject != null)
			{
				var otherObj = collision.Other.Collider.GameObject;
				TryToPickup(otherObj);
			}
		}
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
        base.OnStart();
        // Start gun loaded.
        CurrentAmmo = MaxAmmo;
        CurrentReserves = MaxReserves;
	}

    protected override void OnUpdate()
    {
        if(!animsInited)
        {
			if (ViewmodelRenderer != null)
			{
				animsInited = true;
				ViewmodelRenderer.OnGenericEvent += HandleGenericAnimEvent;
				if (ViewmodelRenderer.SceneModel != null)
				{
					ViewmodelRenderer.SceneModel.SetAnimParameter("FireRate", FireRate);
				}
			}
		}
        if (!IsProxy)
        {
            // Debug draw the last projectile fired.
            Gizmo.Draw.SolidSphere(lastTraceResult.EndPosition, 2.0f);
        }
    }

	private void HandleGenericAnimEvent(SceneModel.GenericEvent eventData)
	{
		if (eventData.Type == "ShootEnd")
		{
            isFiring = false;
		}
		if (eventData.Type == "ReloadEnd")
		{
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
			isReloading = false;
			Info("Reloaded!");
		}
	}



	public virtual void Fire()
    {
		if (ViewmodelRenderer != null)
		{
			if (ViewmodelRenderer.SceneModel != null)
			{
				ViewmodelRenderer.SceneModel.SetAnimParameter("FireRate", FireRate);
				ViewmodelRenderer.SceneModel.SetAnimParameter("ReloadRate", ReloadSpeed);
			}
			else
			{
				Log.Info("No Scenemodel!");
			}
		}
		else
		{
			Log.Info("No viewmodel renderer!");
		}

        if (!isFiring && !isReloading)
        {
            if (CurrentAmmo <= 0)
            {
                Info("Empty!");
            }
            else
            {
				isFiring = true;
				CurrentAmmo--;
                Info("Fired!");

                AnimateFire();
                FireSound();
                PlayerCameraController.Recoil(RecoilStrength);

                float dist = 10000.0f;
                var head = PlayerWeaponController.Head;

                // If the parent has a head shoot out of that. Otherwise shoot out of this gun directly.
                var origin = (head != null) ? head.Transform.Position : head.Transform.Position;
                var lastTraceResult = Scene.Trace.Ray(new Ray(origin, Transform.Rotation.Forward * dist), dist).UseHitboxes().Run();
				var lastTraceResults = Scene.Trace.Ray(new Ray(origin, Transform.Rotation.Forward * dist), dist).UseHitboxes().RunAll();
				//lastTrace.UseHitboxes(true);
				//lastTrace.WithAnyTags("unit");
				//lastTrace.ig
				//lastTraceResult = lastTrace.Run();


				if (lastTraceResult.Hit)
                {
                    Log.Info($"Hit: {lastTraceResult.GameObject} at {lastTraceResult.EndPosition}");
                    Log.Info(lastTraceResult.Hitbox);

                    foreach(var i in lastTraceResults)
                    {
                        Log.Info(i.Component);
                    }

					if (lastTraceResult.Hitbox != null)
                    {
                        Log.Info("This is a Unit");
						Log.Info("His this component: " + lastTraceResult.Component);
                        foreach(string tag in lastTraceResult.Hitbox.Tags)
                        {
                            Log.Info(tag);
                        }
						if (lastTraceResult.Hitbox.Tags.Contains("head"))
                        {
							Log.Info("Headshot!");
							lastTraceResult.Component.GameObject.Components.Get<SIUUnit>().takeDamage(Damage * HEADSHOT_MULTIPLIER, GameObject.Parent.Parent);
                        }
                        else
                        {
							lastTraceResult.Component.GameObject.Components.Get<SIUUnit>().takeDamage(Damage, GameObject.Parent.Parent);
						}
                    }
                }
            }
        }
    }

    [Broadcast] private void FireSound()
    {
        Sound.Play(FireSoundEvent, Transform.Position);
    }



    // HL2 Style reloading. Pool of reserve bullets that refill a fixed magazine size. 


    public void Reload()
    {
		if (ViewmodelRenderer != null)
		{
			if (ViewmodelRenderer.SceneModel != null)
			{
				ViewmodelRenderer.SceneModel.SetAnimParameter("ReloadRate", ReloadSpeed);
			}
			else
			{
				Log.Info("No Scenemodel!");
			}
		}
		else
		{
			Log.Info("No viewmodel renderer!");
		}

		if (!isReloading && !isFiring)
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
                isReloading = true;
                AnimateReload();
            }
        }
    }



    protected void Info(string str)
    {
        Log.Info($"{str} [{CurrentAmmo} / {CurrentReserves} ]");
    }

    private void TryToPickup(GameObject other)
    {
        if (ShouldDropOwnershipOnCollide)
        {
            Network.DropOwnership();
            ShouldDropOwnershipOnCollide = false;
        }

        var playerWeaponController = other.Components.Get<FPSWeaponController>();
        var playerCameraController = other.Components.Get<FPSCameraController>();
        if (playerWeaponController != null && playerCameraController != null)
        {
            Pickup(other, playerWeaponController, playerCameraController);
        }
    }

    private void ShowViewmodel()
    {
        ViewmodelRenderer.Enabled = true;
        WorldmodelRenderer.Enabled = false;

    }
    private void ShowWorldmodel()
    {
        ViewmodelRenderer.Enabled = false;
        WorldmodelRenderer.Enabled = true;

    }

    [Broadcast]
    private void AnimateFire()
    {
        var renderer = (!IsProxy) ? ViewmodelRenderer : WorldmodelRenderer;
        renderer.SceneModel.SetAnimParameter("OnFire", true);
    }

    [Broadcast]
    private void AnimateReload()
    {
        var renderer = (!IsProxy) ? ViewmodelRenderer : WorldmodelRenderer;
        renderer.SceneModel.SetAnimParameter("OnReload", true);
    }
}
