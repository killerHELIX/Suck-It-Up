using Sandbox.UI;

public class MainMenuComponent : Component
{
	public enum MenuPanelType
	{
		MAIN,
		JOINEDLOBBY,
		LOBBYLIST,
		SETTINGS,
		NULL
	}

	[Property] MainMenu MainPanel { get; set; }
	[Property] public SIUJoinedLobbyPanel JoinedLobbyPanel { get; set; }
	[Property] public SIULobbyListPanel LobbyListPanel { get; set; }
	[Property] public SIUSettingsPanel SettingsPanel { get; set; }
	[Property] ScreenPanel myScreenPanel { get; set; }

	[Property] public ulong MyServID { get; set; }

	private MenuPanelType activePanel = MenuPanelType.MAIN;

	private static MainMenuComponent _local = null;

	public static MainMenuComponent Local
	{
		get
		{
			if (!_local.IsValid())
			{
				//_local = Game.ActiveScene.Directory.FindByName("Main Menu").First().Components.Get<MainMenuComponent>();
				_local = Game.ActiveScene.GetAllComponents<MainMenuComponent>().FirstOrDefault(x => x.Network.IsOwner);
				Log.Info("Tried getting local main menu, found this: " + _local);
			}
			return _local;
		}
	}

	protected override void OnStart()
	{
		if (Network.IsProxy){ 
			myScreenPanel.Enabled = false;
			JoinedLobbyPanel.Enabled = true;
			//Enabled = false;
			return; 
		}
		setActivePanel(MenuPanelType.MAIN);
	}

	//protected override void OnUpdate()
	//{
	//	getPanelFromEnum(activePanel).StateHasChanged();
	//}

	protected override async void OnUpdate()
	{
		//Log.Info("Networking ID: " Networking.Id);


		if (MyServID == 0)
		{
			var lobbies = await Networking.QueryLobbies();
			foreach (Sandbox.Network.LobbyInformation lobby in lobbies)
			{
				//Log.Info("Lobby ID: " + lobby.LobbyId);
				//Log.Info("Lobby Owner ID: " + lobby.OwnerId);
				if (lobby.OwnerId == Connection.Local.SteamId)
				{
					this.MyServID = lobby.LobbyId;
					Log.Info(MyServID);
				}
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
