using Sandbox.Network;
using System;
using System.Threading.Tasks;
namespace Sandbox;

[Title("Suck It Up Network Helper")]
[Category("Networking")]
[Icon("electrical_services")]

public sealed class SIUNetworkHelper : Component, Component.INetworkListener
{
	/// <summary>
	/// Create a server (if we're not joining one)
	/// </summary>
	[Property] public bool StartServer { get; set; } = true;

	/// <summary>
	/// The prefab to spawn for the player to control.
	/// </summary>
	[Property] public GameObject RTSPlayerPrefab { get; set; }
	[Property] public GameObject SurvivorPlayerPrefab { get; set; }
	[Property] public GameObject SpectatorPlayerPrefab { get; set; }

	/// <summary>
	/// A list of points to choose from randomly to spawn the player in. If not set, we'll spawn at the
	/// location of the NetworkHelper object.
	/// </summary>
	[Property] public List<GameObject> RTSSpawnPoints { get; set; }
	[Property] public List<GameObject> SurvivorSpawnPoints { get; set; }
	[Property] public List<GameObject> SpectatorSpawnPoints { get; set; }

	private static SIUNetworkHelper _local = null;

	public const string OBJECT_NAME = "Network Helper";


	public static SIUNetworkHelper Local
	{
		get
		{
			if (!_local.IsValid())
			{
				Log.Info("Attempting get local game state");
				if(Game.ActiveScene.Directory.FindByName(OBJECT_NAME).FirstOrDefault(x => x.Network.IsOwner) != null)
				{
					_local = Game.ActiveScene.Directory.FindByName(OBJECT_NAME).FirstOrDefault(x => x.Network.IsOwner).Components.Get<SIUNetworkHelper>();
				}
				else
				{
					Log.Info("No network helper found! We must have aborted the game");
					return null;
				}
				Log.Info("Tried getting local game state, found this: " + _local);
			}
			return _local;
		}
	}

	protected override async Task OnLoad()
	{
		if (Scene.IsEditor)
			return;

		if (StartServer && !GameNetworkSystem.IsActive)
		{
			LoadingScreen.Title = "Creating Lobby";
			await Task.DelayRealtimeSeconds(0.1f);
			GameNetworkSystem.CreateLobby();
		}
	}

	/// <summary>
	/// A client is fully connected to the server. This is called on the host.
	/// </summary>
	public void OnActive(Connection channel)
	{

		if (RTSPlayerPrefab is null || SurvivorPlayerPrefab is null || SpectatorPlayerPrefab is null)
			return;

		GameState.PlayerType pType = GameState.Local.getPlayerTypeFromPlayer(channel.DisplayName);
		Log.Info(channel.DisplayName);

		//
		// Find a spawn location for this player
		//
		var startLocation = FindSpawnLocation(pType).WithScale(1);

		var player = new GameObject();
		// Spawn this object and make the client the owner
		switch (pType)
		{
			case GameState.PlayerType.RTS:
				Log.Info($"Player '{channel.DisplayName}' has joined the game, spawning as a " + RTSPlayerPrefab);
				player = RTSPlayerPrefab.Clone(startLocation, name: $"Player - {channel.DisplayName}");
				break;
			case GameState.PlayerType.SURVIVOR:
				Log.Info($"Player '{channel.DisplayName}' has joined the game, spawning as a " + SurvivorPlayerPrefab);
				player = SurvivorPlayerPrefab.Clone(startLocation, name: $"Player - {channel.DisplayName}");
				break;
			case GameState.PlayerType.SPECTATOR:
				Log.Info($"Player '{channel.DisplayName}' has joined the game, spawning as a " + SpectatorPlayerPrefab);
				player = SpectatorPlayerPrefab.Clone(startLocation, name: $"Player - {channel.DisplayName}");
				break;
		}
		player.NetworkSpawn(channel);
	}

	/// <summary>
	/// Find the most appropriate place to respawn
	/// </summary>
	Transform FindSpawnLocation(GameState.PlayerType pType)
	{
		var spawnList = new List<GameObject>();
		switch (pType)
		{
			case GameState.PlayerType.RTS:
				spawnList = RTSSpawnPoints;
				break;
			case GameState.PlayerType.SURVIVOR:
				spawnList = SurvivorSpawnPoints;
				break;
			case GameState.PlayerType.SPECTATOR:
				spawnList = SpectatorSpawnPoints;
				break;
		}

		//
		// If they have spawn point set then use those
		//
		if (spawnList is not null && spawnList.Count > 0)
		{
			return Random.Shared.FromList(spawnList, default).Transform.World;
		}

		//
		// If we have any SpawnPoint components in the scene, then use those
		//
		var spawnPoints = Scene.GetAllComponents<SpawnPoint>().ToArray();
		if (spawnPoints.Length > 0)
		{
			return Random.Shared.FromArray(spawnPoints).Transform.World;
		}

		//
		// Failing that, spawn where we are
		//
		return Transform.World;
	}
}