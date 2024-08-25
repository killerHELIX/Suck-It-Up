using System;

public class SpectatorPlayer : Component
{
	public static SpectatorPlayer Local
	{
		get
		{
			if (!_local.IsValid())
			{
				_local = Game.ActiveScene.GetAllComponents<SpectatorPlayer>().FirstOrDefault(x => x.Network.IsOwner);
			}
			return _local;
		}
	}

	[Property] public int Team;
	[Property] public SpectatorControl Controller;


	private static SpectatorPlayer _local = null;

	protected override void OnStart()
	{
		//Set Team
		this.Team = 2;

		if (Network.IsProxy) 
		{ 
			return;
		}

		//Update display for all units (probably wont work for those it doesnt have ownership of)
		var allUnitList = Game.ActiveScene.GetAllComponents<Unit>();
		foreach(var unit in allUnitList)
		{
			//Log.Info("Updating Display vars for " + unit.GameObject.Name);
			unit.onTeamChange();
		}

		base.OnStart();

	}
}
