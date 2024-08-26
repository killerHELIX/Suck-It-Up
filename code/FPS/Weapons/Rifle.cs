
public class Rifle : Weapon
{
    [Property] public override string Name { get; set; } = ".357 - \"The Rusty Repeater\"";
    [Property] public override Vector3 ViewmodelPosition { get; set; } = new Vector3(16, 9, -14); // 16,9,-14
    [Property] public override Vector3 HolsterPosition { get; set; } = new Vector3(-10, 0, 45); // -10,0,45
    [Property] public override Rotation HolsterRotation { get; set; } = new Rotation(0.4055798f, 0.4055798f, -0.579228f, 0.579228f); // 0.4055798,0.4055798,-0.579228,0.579228
    [Property] public override float DropDistance { get; set; } = 30f;
    [Property] public override float DropSpeedUp { get; set; } = 10_000f;
    [Property] public override float DropSpeedForward { get; set; } = 3000f;
    [Property] public override int MaxAmmo { get; set; } = 5;
    [Property] public override int MaxReserves { get; set; } = 25;
    [Property] public override float FireRate { get; set; } = 1.5f;
    [Property] public override float ReloadSpeed { get; set; } = 4.0f;
	[Property] public override int Damage { get; set; } = 20;

}
