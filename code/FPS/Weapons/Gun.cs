// using System.Timers;

public abstract class Gun : Weapon
{
    [Property] 
	[Description("The maximum ammount of ammo in this gun's magazine.")]
	public abstract int MaxAmmo { get; set; }

    [Property] 
	[Description("The maximum amount of ammo this gun can hold in its reserve ammo pool.")]
	public abstract int MaxReserves { get; set; }

    [Property] 
	[Description("The rate of fire of this gun in seconds.")]
	public abstract float FireRate { get; set; }
    [Property] 
	[Description("The reload speed of this gun in seconds.")]
	public abstract float ReloadSpeed { get; set; }

    private int CurrentAmmo;
    private int CurrentReserves;
    private float LastFireTime = 0f;
    private float LastReloadTime = 0f;
	private SceneTraceResult lastTraceResult;
    public Gun(){
        // Start gun loaded.
        CurrentAmmo = MaxAmmo;
        CurrentReserves = MaxReserves;
    }

	protected override void OnUpdate()
	{
        base.OnUpdate();

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

    // The Gun is "Paused", i.e. ignoring player input, while certain actions are occurring like fire rate cooldown and reloading.
    private bool IsPaused()
    {
        bool firing = Time.Now - LastFireTime < FireRate;
        bool reloading = Time.Now - LastReloadTime < ReloadSpeed;
        return firing || reloading;
    }


    public override void Fire()
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
                var cam = GameObject.Parent.Components.GetInChildrenOrSelf<CameraComponent>();

                // If the parent has a camera (e.g. a Player) shoot out of that. Otherwise shoot out of this gun directly.
                var origin = (cam != null) ? cam.Transform.Position : cam.Transform.Position;
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

}
