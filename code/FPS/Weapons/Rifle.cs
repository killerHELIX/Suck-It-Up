
public class Rifle : Gun
{
    [Property] public Vector3 ViewmodelPosition { get; set; } = Vector3.Zero; // 16,9,-14
    [Property] public Vector3 HolsterPosition { get; set; } = Vector3.Zero; // -10,0,45
    [Property] public Rotation HolsterRotation { get; set; } = Angles.Zero; // 0.4055798,0.4055798,-0.579228,0.579228
	[Property] public override float DropDistance { get; set; } = 40f;
	[Property] public override float DropSpeedUp { get; set; } = 1000f;
	[Property] public override float DropSpeedForward { get; set; } = 5000f;

	public readonly int MAX_RESERVES = 25;
	public readonly int MAX_AMMO = 5;

	public Rifle()
	{
		Log.Info("Instantiated Rifle");
	}

	public override int GetMaxAmmo()
	{
		return MAX_AMMO;
	}

	public override int GetMaxReserves()
	{
		return MAX_RESERVES;
	}

    public override Vector3 GetHolsterPosition()
    {
        return HolsterPosition;
    }


    public override Rotation GetHolsterRotation()
    {
        return HolsterRotation;
    }

    public override Vector3 GetViewmodelPosition()
    {
        return ViewmodelPosition;
    }
}
