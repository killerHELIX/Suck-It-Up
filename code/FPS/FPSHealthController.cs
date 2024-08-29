using Sandbox;

public sealed class FPSHealthController : Component
{
	[Property] public int MaxHealth { get; set; } = 100;
	[Property] public int CurrentHealth { get; set; } = 100;
	public bool IsAlive
	{
		get {
			return CurrentHealth > 0;
		}
	}

	public void Damage(int amount)
	{

		Log.Info($"Damaging {amount}");
		CurrentHealth = (CurrentHealth - amount).Clamp(0, MaxHealth);
		if (CurrentHealth == 0) Die();

	}

	public void Heal(int amount)
	{
		Log.Info($"Healing {amount}");
		CurrentHealth = (CurrentHealth + amount).Clamp(0, MaxHealth);
	}

	public void Die()
	{
		GameState.Local.addPlayerToList(Connection.Local.DisplayName, GameState.PlayerType.SPECTATOR, true);
		var spawner = SIUNetworkHelper.Local;
		if (spawner != null)
		{
			var spec = spawner.SpectatorPlayerPrefab.Clone(
				parent: Scene, 
				position: Transform.Position + (Transform.Rotation.Up * 3f), 
				rotation: Transform.Rotation, 
				scale: Vector3.One);

			spec.Name = $"Spectator - {Network.OwnerConnection.DisplayName}, has died";
			//spec.NetworkSpawn();
		}

		GameObject.Destroy();

	}

	protected override void OnUpdate()
	{
		if (IsProxy) return;
	}
}
