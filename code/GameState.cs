using Sandbox.Network;
using System.Threading;

public class GameState : Component
{

	[Property] public CorpseList GameCorpseList { get; set; }
	[Property] public EndGamePanel EndGamePanel { get; set; }
	[Property] public ScreenPanel ScreenPanel { get; set; }

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
		SURVIVOR,
		NONE
	}

	private static GameState _local = null;

	[Sync] [Property] public List<string> rtsPlayerList { get; set; } = new List<string>();
	[Sync] public List<string> spectatorPlayerList { get; set; } = new List<string>();
	[Sync] public List<string> survivorPlayerList { get; set; } = new List<string>();
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
			//pullCurrentGameStateFromHost();
			// DEBUG
			//survivorPlayerList.Add("balls");
			rtsPlayerList.Add("Grundle");
			// DEBUG
			//Log.Info(Network.OwnerConnection);
			//Log.Info(Network.OwnerConnection.DisplayName);
			spectatorPlayerList.Add(Network.OwnerConnection.DisplayName);
			GameObject.NetworkSpawn(Network.OwnerConnection);
		}
		else 
		{
			_local = this;
			if(GameState.Local != null)
			{
				string myName = Connection.Local.DisplayName;
				if (getPlayerTypeFromPlayer(Connection.Local.DisplayName) == PlayerType.NONE)
				{
					Log.Info("Player is not on any team!");
					addPlayerToList(myName, PlayerType.SPECTATOR, true);
				}
				else
				{
					Log.Info("Player is on a team already!");
				}
			}
			else
			{
				Log.Info("GameState.Local is null!");
				addPlayerToList("Unknown PLayer", PlayerType.SPECTATOR, true);
			}
		}
	}

	protected override void OnStart()
	{
		//survivorPlayerList.Add("balls");
		//spectatorPlayerList.Add(Network.OwnerConnection.DisplayName);
	}

	[Broadcast] public void addPlayerToList(string player, PlayerType listType, bool fromPlayer)
	{
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

	public void startGame()
	{
		if (rtsPlayerList.Count() > 0 && survivorPlayerList.Count() > 0)
		{
			currentGameState = GameStateType.PLAYING;
			Game.ActiveScene.LoadFromFile("scenes/sui_main.scene");
			return;
		}
	}

	[Broadcast] public void setPhase(int phase)
	{
		Log.Info("Set phase to: " + phase);
		matchPhase = phase;
	}

	[Broadcast] public void finishGame()
	{
		ScreenPanel.Enabled = true;
		EndGamePanel.Enabled = true;
		currentGameState = GameStateType.FINISHED;
		//PLAY SOUND HERE
		//Implement wait somehow
		//Thread.Sleep(10000);
		currentGameState = GameStateType.MENU;
		GameNetworkSystem.Disconnect();
		Game.ActiveScene.LoadFromFile("scenes/main_menu.scene");

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

	public PlayerType getPlayerTypeFromPlayer(string playername)
	{
		if (rtsPlayerList.Contains(playername))
		{
			return PlayerType.RTS;
		}
		else if (survivorPlayerList.Contains(playername))
		{
			return PlayerType.SURVIVOR;
		}
		else if(spectatorPlayerList.Contains(playername))
		{
			return PlayerType.SPECTATOR;
		}
		else
		{
			return PlayerType.NONE;
		}
	}
}
