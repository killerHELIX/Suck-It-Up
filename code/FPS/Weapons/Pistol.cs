
public class Pistol : Component, IWeapon
{
    public int Mags = 5;
    public int MagCap = 8;

    public int CurrentMagCap;
	private SceneTraceResult lastTraceResult;
    public Pistol(){
        Log.Info("Instantiated Pistol");
        CurrentMagCap = MagCap;
    }
    
	protected override void OnUpdate()
	{
		if ( !IsProxy )
		{
		// Debug draw the last projectile fired.
		Gizmo.Draw.SolidSphere(lastTraceResult.EndPosition, 2.0f);
		}
	}

    public void fire()
    {
        if (CurrentMagCap <= 0) 
        {
            Log.Info($"Empty! [{CurrentMagCap} / {MagCap} || {Mags} ]");
        }
        else 
        {
            CurrentMagCap--;
            Log.Info($"Fired! [{CurrentMagCap} / {MagCap} || {Mags} ]");

			float dist = 10000.0f;
			lastTraceResult = Scene.Trace.Ray( new Ray(Transform.Position, Transform.Rotation.Forward * dist), dist ).Run();

			if ( lastTraceResult.Hit )
			{
				Log.Info( $"Hit: {lastTraceResult.GameObject} at {lastTraceResult.EndPosition}" );
			}
        }
    }

    public void reload()
    {
        if (CurrentMagCap == MagCap) 
        {
            Log.Info($"Already reloaded! [{CurrentMagCap} / {MagCap} || {Mags} ]");
        }
        else if (Mags <= 0) 
        {
            Log.Info($"Out of mags! [{CurrentMagCap} / {MagCap} || {Mags} ]");
        } 
        else
        {
            Mags--;
            CurrentMagCap = MagCap;
            Log.Info($"Reloaded! [{CurrentMagCap} / {MagCap} || {Mags} ]");
        }
    }

}