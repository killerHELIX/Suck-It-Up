using Sandbox;

public sealed class FPSHealthController : Component
{
	[Property] public int MaxHealth { get; set; } = 100;
	[Property] public int CurrentHealth { get; set; } = 100;
	public bool IsAlive
	{
		get {
			return CurrentHealth <= 0;
		}
	}

	public void Damage(int amount)
	{

		Log.Info($"Damaging {amount}");
		CurrentHealth = (CurrentHealth - amount).Clamp(0, MaxHealth);

	}

	public void Heal(int amount)
	{
		Log.Info($"Healing {amount}");
		CurrentHealth = (CurrentHealth + amount).Clamp(0, MaxHealth);
	}

	protected override void OnUpdate()
	{
		Log.Info($"Health: [ {CurrentHealth} / {MaxHealth} ]");

		if (Input.Pressed("Damage")) Damage(1);

		if (Input.Pressed("Heal")) Heal(1);

	}
}
