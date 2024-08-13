using System;

public abstract class Gun : Weapon
{

    private int CurrentAmmo;
    private int CurrentReserves;
	private SceneTraceResult lastTraceResult;
    public Gun(){
        Log.Info("Instantiated Gun");
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
        else
        {
            CurrentAmmo--;
            Info("Fired!");

            float dist = 10000.0f;
            // Shoot bullets directly out of the eyes (camera) of the parent (player)
            var cam = GameObject.Parent.Components.GetInChildren<CameraComponent>();
            Log.Info(cam);
            if (cam != null)
            {
                Log.Info("xxxxxxxxxxx");
                var origin = cam.Transform.Position;
                lastTraceResult = Scene.Trace.Ray(new Ray(origin, Transform.Rotation.Forward * dist), dist).Run();

                if (lastTraceResult.Hit)
                {
                    Log.Info($"Hit: {lastTraceResult.GameObject} at {lastTraceResult.EndPosition}");
                }
            }
        }
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
        else
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
        }
    }

    protected void Info(string str)
    {
        Log.Info($"{str} [{CurrentAmmo} / {CurrentReserves} ]");
    }

}
