// using System.Timers;

public abstract class Gun : Weapon
{
	[Property] public abstract int MaxAmmo { get; set; }
	[Property] public abstract int MaxReserves { get; set; }

    private int CurrentAmmo;
    private int CurrentReserves;
    private bool IsFiring = false;
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


            // Debug draw the last projectile fired.
            Gizmo.Draw.SolidSphere(lastTraceResult.EndPosition, 2.0f);
		}
	}


    public override void Fire()
    {

        if (CurrentAmmo <= 0)
        {
            Info("Empty!");
        }
        else if (!IsFiring)
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

    // HL2 Style reloading. Pool of reserve bullets that refill a fixed magazine size. 
    public void Reload()
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

    protected void Info(string str)
    {
        Log.Info($"{str} [{CurrentAmmo} / {CurrentReserves} ]");
    }

}
