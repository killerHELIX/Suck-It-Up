﻿using Sandbox.UI;

public class GameState : Component
{

	public const int MAX_RTS_PLAYERS = 1;
	public const int MAX_SPECTATOR_PLAYERS = 99;
	public const int MAX_SURVIVOR_PLAYERS = 10;
	public const string OBJECT_NAME = "Game State";

	public enum GameStateType
	{
		MENU,
		PLAYING,
		FINISHED
	}
	public enum PlayerType
	{
		RTS,
		SPECTATOR,
		SURVIVOR
	}

	private static GameState _local = null;

	[Sync] public List<string> rtsPlayerList { get; set; }
	[Sync] public List<string> spectatorPlayerList { get; set; }
	[Sync] public List<string> survivorPlayerList { get; set; }
	[Sync] public int matchPhase {  get; set; }
	[Sync] public GameStateType currentGameState {get; set;}

	public static GameState Local
	{
		get
		{
			if (!_local.IsValid())
			{
				Log.Info("Attempting get local game state");
				_local = Game.ActiveScene.Directory.FindByName(OBJECT_NAME).FirstOrDefault(x => x.Network.IsOwner).Components.Get<GameState>();
				//_local = Game.ActiveScene.GetAllComponents<MainMenuComponent>().FirstOrDefault(x => x.Network.IsOwner);
				Log.Info("Tried getting local game state, found this: " + _local);
			}
			return _local;
		}
	}

	protected override void OnAwake()
	{
		GameObject.Flags |= GameObjectFlags.DontDestroyOnLoad;

		Log.Info("I am a proxy: " + Network.IsProxy);

		if(!Network.IsProxy)
		{
			Log.Info("Non proxy gamestate starting...");
			Network.TakeOwnership();
			pullCurrentGameStateFromHost();
			survivorPlayerList.Add("balls");
			spectatorPlayerList.Add(Network.OwnerConnection.DisplayName);
			GameObject.NetworkSpawn(Network.OwnerConnection);
		}
		else 
		{
			addPlayerToList(Connection.Local.DisplayName, PlayerType.SPECTATOR, true);
			_local = this;
		}
	}

	protected override void OnStart()
	{
		//survivorPlayerList.Add("balls");
		//spectatorPlayerList.Add(Network.OwnerConnection.DisplayName);
	}

	[Broadcast] public void addPlayerToList(string player, PlayerType listType, bool fromPlayer)
	{
		// If this call comes from the local player, propagate it to all the other players' states as well
		if (fromPlayer)
		{
			foreach (GameObject playerGameStateObject in Game.ActiveScene.Directory.FindByName(OBJECT_NAME))
			{
				if (!playerGameStateObject.Network.IsOwner)
				{
					GameState playerGameState = playerGameStateObject.Components.Get<GameState>();
					Log.Info("Trying to update " + playerGameState.GameObject.Name + " for player " + playerGameState.Network.OwnerConnection.DisplayName);
					playerGameState.addPlayerToList(player, listType, false);
				}
			}
		}
		Log.Info(player + " clicked " + listType);
		// Don't go above the max players
		if (getPlayerListFromType(listType).Count() >= getMaxPlayerFromType(listType))
		{
			return;
		}
		// Remove the player from each list before adding so we don't have double
		rtsPlayerList.Remove(player);
		spectatorPlayerList.Remove(player);
		survivorPlayerList.Remove(player);
		// Finally add player to the local list
		getPlayerListFromType(listType).Add(player);
	}

	private void pullCurrentGameStateFromHost()
	{
		Log.Info("Getting active scene directory: " + Game.ActiveScene.Directory);
		Log.Info("Game State objects found: " + Game.ActiveScene.Directory.FindByName(OBJECT_NAME).Count());
		Log.Info("Am I the owner?: " + (Game.ActiveScene.Directory.FindByName(OBJECT_NAME).First().Network.IsOwner));
		Log.Info("Owner Connection: " + (Game.ActiveScene.Directory.FindByName(OBJECT_NAME).First().Network.OwnerConnection));
		Log.Info("Game State objects found: " + Game.ActiveScene.Directory.FindByName(OBJECT_NAME).FirstOrDefault(x => x.Network.OwnerConnection.IsHost));
		if(!Networking.IsHost)
		{
			Log.Info("I am not the host, but I got the hosts game state");
			var hostGameStateObject = Game.ActiveScene.Directory.FindByName(OBJECT_NAME).FirstOrDefault(x => x.Network.OwnerConnection.IsHost);
			var hostGameState = hostGameStateObject.Components.Get<GameState>();
			rtsPlayerList = hostGameState.rtsPlayerList;
			spectatorPlayerList = hostGameState.spectatorPlayerList;
			survivorPlayerList = hostGameState.survivorPlayerList;
			matchPhase = hostGameState.matchPhase;
			currentGameState = hostGameState.currentGameState;
		}
		else
		{
			Log.Info("Creating host gamestate");
			rtsPlayerList = new List<string>();
			spectatorPlayerList = new List<string>();
			survivorPlayerList = new List<string>();
			matchPhase = 0;
			currentGameState = GameStateType.MENU;
		}
	}

	public List<string> getPlayerListFromType(PlayerType pType)
	{
		switch (pType)
		{
			case PlayerType.RTS:
				return rtsPlayerList;
			case PlayerType.SPECTATOR:
				return spectatorPlayerList;
			case PlayerType.SURVIVOR:
				return survivorPlayerList;
			default:
				return spectatorPlayerList;
		}
	}

	public int getMaxPlayerFromType(PlayerType pType)
	{
		switch (pType)
		{
			case PlayerType.RTS:
				return MAX_RTS_PLAYERS;
			case PlayerType.SPECTATOR:
				return MAX_SPECTATOR_PLAYERS;
			case PlayerType.SURVIVOR:
				return MAX_SURVIVOR_PLAYERS;
			default:
				return 0;
		}
	}
}