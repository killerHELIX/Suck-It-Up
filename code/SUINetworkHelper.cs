﻿using Sandbox.Network;
using System;
using System.Threading.Tasks;
namespace Sandbox;

[Title("Suck It Up Network Helper")]
[Category("Networking")]
[Icon("electrical_services")]

public sealed class SUINetworkHelper : Component, Component.INetworkListener
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
		Log.Info($"Player '{channel.DisplayName}' has joined the game");

		if (RTSPlayerPrefab is null || SurvivorPlayerPrefab is null || SpectatorPlayerPrefab is null)
			return;

		GameState.PlayerType pType = GameState.Local.getPlayerTypeFromPlayer(channel.DisplayName);


		//
		// Find a spawn location for this player
		//
		var startLocation = FindSpawnLocation().WithScale(1);

		// Spawn this object and make the client the owner
		var player = PlayerPrefab.Clone(startLocation, name: $"Player - {channel.DisplayName}");
		player.NetworkSpawn(channel);
	}

	/// <summary>
	/// Find the most appropriate place to respawn
	/// </summary>
	Transform FindSpawnLocation()
	{
		//
		// If they have spawn point set then use those
		//
		if (SpawnPoints is not null && SpawnPoints.Count > 0)
		{
			return Random.Shared.FromList(SpawnPoints, default).Transform.World;
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

	public List<> getSpawnListFromPlayerType(GameState.PlayerType pType)
	{
		switch(pType)
		{
			case GameState.PlayerType.RTS:
				return RTSSpawnPoints;

		}
	}
}