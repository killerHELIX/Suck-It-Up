public class Pistol : Gun
{
    [Property] public float ViewmodelXOffset { get; set; } = 30f;
    [Property] public float ViewmodelYOffset { get; set; } = 9f;
    [Property] public float ViewmodelZOffset { get; set; } = -10f;
    public readonly int MAX_RESERVES = 33;
    public readonly int MAX_AMMO = 8;

    public Pistol()
    {
        Log.Info("Instantiated Pistol");
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
