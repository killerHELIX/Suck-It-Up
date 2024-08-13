
public class Rifle : Gun
{
	[Property] public float ViewmodelXOffset { get; set; } = 25f;
	[Property] public float ViewmodelYOffset { get; set; } = 11f;
	[Property] public float ViewmodelZOffset { get; set; } = -17f;
	public static readonly int MAX_RESERVES = 15;
	public static readonly int MAX_AMMO = 8;

	public Rifle()
	{
		Log.Info("Instantiated Rifle");
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

	public override int GetMaxAmmo()
	{
		return MAX_AMMO;
	}

	public override int GetMaxReserves()
	{
		return MAX_RESERVES;
	}
}
