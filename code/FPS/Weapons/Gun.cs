using System;
using System.Diagnostics;
using System.Threading;

public abstract class Gun : Weapon
{

    private int CurrentAmmo;
    private int CurrentReserves;
	private readonly int FIRE_TIME_MS = 500;
	private readonly int RELOAD_TIME_MS = 5000;
	private Stopwatch FiringStopwatch = new Stopwatch();
	private Stopwatch ReloadingStopwatch = new Stopwatch();
	private SceneTraceResult lastTraceResult;
    public Gun(){
        CurrentAmmo = GetMaxAmmo();
        CurrentReserves = GetMaxReserves();
    }

	protected override void OnUpdate()
	{
		if ( !IsProxy )
		{

            // Debug draw the last projectile fired.
            Gizmo.Draw.SolidSphere(lastTraceResult.EndPosition, 2.0f);
		}
	}

	public abstract int GetMaxAmmo();
	public abstract int GetMaxReserves();

    public override void Fire()
    {
        if (CurrentAmmo <= 0)
        {
            Info("Empty!");
        }
        else if (CanFire())
        {

            CurrentAmmo--;
            Info("Fired!");

            float dist = 10000.0f;
            var cam = GameObject.Parent.Components.GetInChildren<CameraComponent>();

			// If the parent has a camera (e.g. a Player) shoot out of that. Otherwise shoot out of this gun directly.
			var origin = (cam != null) ? cam.Transform.Position : cam.Transform.Position;
			lastTraceResult = Scene.Trace.Ray(new Ray(origin, Transform.Rotation.Forward * dist), dist).Run();

			if (lastTraceResult.Hit)
			{
				Log.Info($"Hit: {lastTraceResult.GameObject} at {lastTraceResult.EndPosition}");
			}

			StartFireCooldown();
        }
    }

    private void StartFireCooldown()
    {
		FiringStopwatch.Start();
    }

    private bool CanFire()
    {
		var firingMsElapsed = FiringStopwatch.ElapsedMilliseconds;
		var reloadingMsElapsed = ReloadingStopwatch.ElapsedMilliseconds;
		Log.Info($"Fire cooldown: {firingMsElapsed}ms / {FIRE_TIME_MS}ms");

		var isFiring = firingMsElapsed != 0 && firingMsElapsed < FIRE_TIME_MS;
		var isReloading = reloadingMsElapsed != 0 && reloadingMsElapsed < RELOAD_TIME_MS;

        if (!isFiring && !isReloading)
		{
			FiringStopwatch.Reset();
			Log.Info("Can Fire");
			return true;
		}

		Log.Info("Cannot Fire");
		return false;
    }

    // HL2 Style reloading. Pool of reserve bullets that refill a fixed magazine size. 
    public void Reload()
    {
        if (CurrentAmmo == GetMaxAmmo())
        {
            Info("Already reloaded!");
        }
        else if (CurrentReserves <= 0)
        {
            Info("Out of reserves!");
        }
        else if (CanReload())
        {
            var ammoDiff = GetMaxAmmo() - CurrentAmmo;

            // If ammoDiff is less than reserves, reload full mag.
            if (ammoDiff <= CurrentReserves)
            {
                CurrentReserves -= ammoDiff;
                CurrentAmmo = GetMaxAmmo();
            }
            // Else, ammoDiff is greater than reserves, so load mag with whatever is left in reserves.
            else
            {
                CurrentAmmo += CurrentReserves;
                CurrentReserves -= CurrentReserves;
            }

            Info("Reloaded!");
			StartReloadCooldown();
        }
    }

    private bool CanReload()
    {
		var reloadingMsElapsed = ReloadingStopwatch.ElapsedMilliseconds;
		Log.Info($"Reload cooldown: {reloadingMsElapsed}ms / {RELOAD_TIME_MS}ms");

		var isReloading = reloadingMsElapsed != 0 && reloadingMsElapsed < RELOAD_TIME_MS;

        if (!isReloading)
		{
			ReloadingStopwatch.Reset();
			Log.Info("Can Reload");
			return true;
		}

		Log.Info("Cannot Reload");
		return false;
    }


    private void StartReloadCooldown()
    {
		ReloadingStopwatch.Start();
    }

    protected void Info(string str)
    {
        Log.Info($"{str} [{CurrentAmmo} / {CurrentReserves} ]");
    }

}
