public class Pistol : Gun
{
    [Property] public override Vector3 ViewmodelPosition { get; set; } = new Vector3(15, 5, -6); // 15,5,-6
    [Property] public override Vector3 HolsterPosition { get; set; } = new Vector3(1, -9, 30); // 1,-9,30
    [Property] public override Rotation HolsterRotation { get; set; } = new Rotation(0, 0.6755902f, 0, 0.7372773f); // 0,0.6755902,0,0.7372773
    [Property] public override float DropDistance { get; set; } = 20f;
    [Property] public override float DropSpeedUp { get; set; } = 500f;
    [Property] public override float DropSpeedForward { get; set; } = 1000f;
	[Property] public override int MaxAmmo { get; set; } = 8;
	[Property] public override int MaxReserves { get; set; } = 33;

    public Pistol()
    {
        Log.Info("Instantiated Pistol");
    }
}
