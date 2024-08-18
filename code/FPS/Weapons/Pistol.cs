public class Pistol : Gun
{
    [Property] public Vector3 ViewmodelPosition { get; set; } = Vector3.Zero; // 30,7,-10
    [Property] public Vector3 HolsterPosition { get; set; } = Vector3.Zero; // 1,-9,30
    [Property] public Rotation HolsterRotation { get; set; } = Angles.Zero; // 0,0.6755902,0,0.7372773
    public readonly int MAX_RESERVES = 33;
    public readonly int MAX_AMMO = 8;

    public Pistol()
    {
        Log.Info("Instantiated Pistol");
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
