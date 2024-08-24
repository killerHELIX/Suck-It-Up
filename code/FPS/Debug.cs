using Sandbox;

public sealed class Debug : Component
{
	[Property] GameObject PlayerPrefab;
	[Property] GameObject Spawn;

	protected override void OnAwake()
	{
		Log.Info("SPAWNING PLAYER!!!!!!!!!!!!!!!");
		var clone = PlayerPrefab.Clone();
		clone.SetParent(Scene, keepWorldPosition: false);
		clone.Transform.Position = Spawn.Transform.Position;
		// clone.Network.TakeOwnership();
		// clone.MakeNameUnique();
		clone.NetworkSpawn();
		// clone.Network.Refresh();
	}
	protected override void OnUpdate()
	{

	}
}
