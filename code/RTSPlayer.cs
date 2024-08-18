using System;

public class RTSPlayer : Component
{
	public static RTSPlayer Local
	{
		get
		{
			if(!_local.IsValid())
			{
				_local = Game.ActiveScene.GetAllComponents<RTSPlayer>().FirstOrDefault(x => x.Network.IsOwner);
			}
			return _local;
		}
	}

	[Property] public PlayerUnitControl UnitControl;
	[Property] public int Team;
	[Property] public RTSGameComponent LocalGame;
	//DEBUG REMOVE
	[Property] public GameObject skeltalPrefab {  get; set; }
	[Property] public GameObject skeltalHousePrefab { get; set; }
	//DEBUG REMOVE

	private List<GameObject> myUnits = new List<GameObject>();
	public uint resourcePoints = 0;
	private float lastRPTickTime = Time.Now;
	public UnitFactory myUnitFactory = new UnitFactory();

	private static RTSPlayer _local = null;

	private const float DEFAULT_RP_RATE = 1f;

	protected override void OnStart()
	{
		if(Network.IsProxy) 
		{ 
			UnitControl.Enabled = false;
			LocalGame.GameHud.Enabled = false;
			LocalGame.Enabled = false;
			return;
		}

		//Set Team. This will probably be replaced when we have a lobby setup
		if (Game.ActiveScene.GetAllComponents<RTSPlayer>().Count() == 1)
		{
			this.Team = 0;
		}
		else
		{
			this.Team = Game.ActiveScene.GetAllComponents<RTSPlayer>().MaxBy(x => x.Team).Team + 1;
		}

		//Update display for all units (probably wont work for those it doesnt have ownership of)
		var allUnitList = Game.ActiveScene.GetAllComponents<Unit>();
		foreach(var unit in allUnitList)
		{
			//Log.Info("Updating Display vars for " + unit.GameObject.Name);
			unit.onTeamChange();
		}

		//Get Ownership over your units
		var myUnitList = Game.ActiveScene.GetAllComponents<Unit>().Where(x => x.team == Team);
		foreach( var unit in myUnitList)
		{
			//Log.Info("Taking Ownership of " + unit.GameObject.Name);
			unit.Network.TakeOwnership();
			unit.onTeamChange();
		}
		//Get Ownership over control orbs
		var myOrbList = Game.ActiveScene.GetAllComponents<ControlOrb>().Where(x => x.team == Team);
		foreach (var orb in myOrbList)
		{
			//Log.Info("Taking Ownership of " + orb.GameObject.Name);
			orb.Network.TakeOwnership();
			orb.onOwnerJoin();
		}

		base.OnStart();

	}

	protected override void OnUpdate()
	{
		if(Time.Now - lastRPTickTime > DEFAULT_RP_RATE)
		{
			resourcePoints++;
			lastRPTickTime = Time.Now;
			Log.Info("Resource Points: " + resourcePoints);
		}

	}

	public void addUnit(GameObject newUnit)
	{
		myUnits.Add( newUnit );
	}

	public void removeUnit(GameObject unitToRemove)
	{
		myUnits.Remove( unitToRemove );
	}
}
