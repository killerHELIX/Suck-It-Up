using Sandbox.Network;
using System.Threading.Tasks;

public class MainMenuComponent : Component, Component.INetworkListener
{
	public enum MenuPanelType
	{
		MAIN,
		JOINEDLOBBY,
		LOBBYLIST,
		SETTINGS,
		NULL
	}

	[Property] public MainMenu MainPanel { get; set; }
	[Property] public SIUJoinedLobbyPanel JoinedLobbyPanel { get; set; }
	[Property] public SIULobbyListPanel LobbyListPanel { get; set; }
	[Property] public SIUSettingsPanel SettingsPanel { get; set; }
	[Property] ScreenPanel myScreenPanel { get; set; }

	[Property] public ulong MyServID { get; set; } = 0;

	[Property] public bool startServer { get; set; } = false;

	private MenuPanelType activePanel = MenuPanelType.MAIN;

	private static MainMenuComponent _local = null;

	private Task<List<LobbyInformation>> currentLobbies = null; 

	public bool joinedGame = false;

	public static MainMenuComponent Local
	{
		get
		{
			if (!_local.IsValid())
			{
				var localFirstTry = Game.ActiveScene.Directory.FindByName("Menu").First().Components.Get<MainMenuComponent>();
				var localSecondTry = Game.ActiveScene.GetAllComponents<MainMenuComponent>().FirstOrDefault(x => x.Network.IsOwner);

				if (localFirstTry != null)
				{
					_local = localFirstTry;
				}
				else if (localSecondTry != null)
				{
					Log.Info("Didn't find correct component inside name=Menu, but did we find the object at all?\n");
					if(Game.ActiveScene.Directory.FindByName("Menu").Count() > 0)
					{
						Log.Info("Yes");
					}
					else
					{
						Log.Info("No");
					}
					_local = localSecondTry;
				}
				else
				{
					Log.Info("Still couldn't find the main menu component for some reason, here's a dump of enabled GameObjects: \n");
					var goList = Game.ActiveScene.GetAllObjects(true);
					foreach(GameObject obj in goList)
					{
						Log.Info(obj.Name);
					}
				}
				Log.Info("Tried getting local main menu, found this: " + _local);
			}
			return _local;
		}
	}

	protected override async Task OnLoad()
	{
	}

	public void OnActive(Connection channel)
	{
	}

	void OnDisconnected(Connection channel)
	{
		GameState.Local.rtsPlayerList.Remove(channel.DisplayName);
		GameState.Local.spectatorPlayerList.Remove(channel.DisplayName);
		GameState.Local.survivorPlayerList.Remove(channel.DisplayName);
	}

	protected override void OnStart()
	{
		//if (Network.IsProxy){
			//Log.Info("Found a Proxy Menu");
			//myScreenPanel.Enabled = false;
			//JoinedLobbyPanel.Enabled = true;
			//Enabled = false;
			//return; 
		//}
		Log.Info("Menu start");
		
		if((Connection.Host != Connection.Local && Connection.Host != null )|| joinedGame)
		{
			setActivePanel(MenuPanelType.JOINEDLOBBY);
		}
		else
		{
			setActivePanel(MenuPanelType.MAIN);
		}
		getPanelFromEnum(activePanel).Panel.PlaySound("sounds/enterthevaccuum.sound");
	}

	//protected override void OnUpdate()
	//{
	//	getPanelFromEnum(activePanel).StateHasChanged();
	//}

	protected override void OnUpdate()
	{
		//Log.Info("serv id: " + MyServID + ", activePanel: " + activePanel);
		if (Scene.IsEditor)
		{
			Log.Info("isEditor");
			return;
		}


		if (MyServID == 0 && activePanel == MenuPanelType.JOINEDLOBBY)
		{
			if(currentLobbies == null)
			{
				Log.Info("Querying lobbies");
				currentLobbies = Networking.QueryLobbies();
			}
			else if(currentLobbies.IsCompleted)
			{
				Log.Info("Got this many lobbies: " + currentLobbies.Result.Count());
				var lobbies = currentLobbies.Result;
				foreach (Sandbox.Network.LobbyInformation lobby in lobbies)
				{
					Log.Info("Lobby ID: " + lobby.LobbyId);
					Log.Info("Lobby Owner ID: " + lobby.OwnerId);
					if (lobby.OwnerId == Connection.Local.SteamId)
					{
						this.MyServID = lobby.LobbyId;
						Log.Info(MyServID);
					}
				}
			}
            else
            {
				Log.Info("Awaiting query");
            }
        }
		getPanelFromEnum(activePanel).StateHasChanged();
	}


	public void setActivePanel(MenuPanelType panel)
	{
		MainPanel.Enabled = false;
		JoinedLobbyPanel.Enabled = false;
		LobbyListPanel.Enabled = false;
		SettingsPanel.Enabled = false;

		if (getPanelFromEnum(panel) != null)
		{
			activePanel = panel;
			getPanelFromEnum(panel).Enabled = true;
		}
	}

	public PanelComponent getPanelFromEnum(MenuPanelType panelEnum)
	{
		switch (panelEnum)
		{
			case MenuPanelType.MAIN:
				return MainPanel;
			case MenuPanelType.JOINEDLOBBY:
				return JoinedLobbyPanel;
			case MenuPanelType.LOBBYLIST:
				return LobbyListPanel;
			case MenuPanelType.SETTINGS:
				return SettingsPanel;
			default:
				Game.Close();
				return null;
		}
	}
}
