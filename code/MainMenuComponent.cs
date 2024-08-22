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
	[Property] SIUJoinedLobbyPanel JoinedLobbyPanel { get; set; }
	[Property] SIULobbyListPanel LobbyListPanel { get; set; }
	[Property] SIUSettingsPanel SettingsPanel { get; set; }
	[Property] ScreenPanel myScreenPanel { get; set; }

	private MenuPanelType activePanel = MenuPanelType.MAIN;

	private static MainMenuComponent _local = null;

	public static MainMenuComponent Local
	{
		get
		{
			if (!_local.IsValid())
			{
				_local = Game.ActiveScene.Directory.FindByName("Main Menu").First().Components.Get<MainMenuComponent>();//Game.ActiveScene.GetAllComponents<MainMenuComponent>().FirstOrDefault(x => x.Network.IsOwner);
				Log.Info("Tried getting local main menu, found this: " + _local);
			}
			return _local;
		}
	}

	protected override void OnStart()
	{
		base.OnStart();
		setActivePanel(MenuPanelType.MAIN);
	}

	protected override void OnUpdate()
	{
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
		else
		{
			activePanel = MenuPanelType.MAIN;
			MainPanel.Enabled = true;
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
				return null;
		}
	}
}
