
public class Rifle : Weapon
{
	[Property] public float ViewmodelXOffset { get; set; } = 25f;
	[Property] public float ViewmodelYOffset { get; set; } = 11f;
	[Property] public float ViewmodelZOffset { get; set; } = -17f;
    public int MaxReserves = 15;
    public int Reserves;

    public int MaxAmmo = 8;
    public int CurrentAmmo;
	private SceneTraceResult lastTraceResult;
    public Rifle(){
        Log.Info("Instantiated Rifle");
        CurrentAmmo = MaxAmmo;
        Reserves = MaxReserves;
    }
    
	protected override void OnUpdate()
	{
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
        else 
        {
            CurrentAmmo--;
            Info("Fired!");

			float dist = 10000.0f;
			lastTraceResult = Scene.Trace.Ray( new Ray(Transform.Position, Transform.Rotation.Forward * dist), dist ).Run();

			if ( lastTraceResult.Hit )
			{
				Log.Info( $"Hit: {lastTraceResult.GameObject} at {lastTraceResult.EndPosition}" );
			}
        }
    }

    // HL2 Style reloading. Pool of reserve bullets that refill a fixed magazine size. 
    public override void Reload()
    {
        if (CurrentAmmo == MaxAmmo) 
        {
            Info("Already reloaded!");
        }
        else if (Reserves <= 0) 
        {
            Info("Out of reserves!");
        } 
        else
        {
            var ammoDiff = MaxAmmo - CurrentAmmo;

            // If ammoDiff is less than reserves, reload full mag.
            if (ammoDiff <= Reserves)
            {
                Reserves -= ammoDiff;
                CurrentAmmo = MaxAmmo;
            } 
            // Else, ammoDiff is greater than reserves, so load mag with whatever is left in reserves.
            else
            {
                CurrentAmmo += Reserves;
                Reserves -= Reserves;
            }

            Info("Reloaded!");
        }
    }

    private void Info(string str)
    {
        Log.Info($"{str} [{CurrentAmmo} / {Reserves} ]");
    }

    public override float GetViewmodelXOffset()
    {
        return ViewmodelXOffset;
    }

    public override float GetViewmodelYOffset()
    {
        return ViewmodelYOffset;
    }

    public override float GetViewmodelZOffset()
    {
        return ViewmodelZOffset;
    }
}
