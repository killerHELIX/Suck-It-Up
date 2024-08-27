using System;

public class RTSPlayer : Component
{
	public static RTSPlayer Local
	{
		get
		{
			if (!_local.IsValid())
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
	[Property] public GameObject skeltalPrefab { get; set; }
	[Property] public GameObject skeltalHousePrefab { get; set; }
	//DEBUG REMOVE
	[Property] public int ResourceCap {  get; set; }
	[Property] public int CapacityCap { get; set; }

	public List<GameObject> myUnits = new List<GameObject>();
	public int resourcePoints = 0;
	public int capacityPoints = 0;
	private float lastRPTickTime = Time.Now;
	public UnitFactory myUnitFactory = new UnitFactory();

	private static RTSPlayer _local = null;

	private const float DEFAULT_RP_RATE = 1f;
	private const float PER_PLAYER_RESOURCE_MULT = 1.5f;
	private const float PER_PHASE_RESOURCE_MULT = 2f;

	protected override void OnStart()
	{
		if(Network.IsProxy) 
		{ 
			UnitControl.Enabled = false;
			LocalGame.ThisScreen.Enabled = false;
			LocalGame.GameHud.Enabled = false;
			LocalGame.Enabled = false;
			return;
		}

		//Set Team
		this.Team = 0;

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
			addUnit(unit.GameObject, unit.CapacityCost);
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
		if(Time.Now - lastRPTickTime > DEFAULT_RP_RATE / float.Max(((PER_PLAYER_RESOURCE_MULT * 0) * (PER_PHASE_RESOURCE_MULT * 0)), 1))
		{
			resourcePoints++;
			lastRPTickTime = Time.Now;
			//Log.Info("Resource Points: " + resourcePoints);
		}

	}

	public void addResourcePoints(int newResPoints)
	{
		if (resourcePoints + newResPoints > 0 && resourcePoints + newResPoints <= ResourceCap)
		{
			resourcePoints += newResPoints;
		}
	}

	public void addCapacityPoints(int newCapPoints)
	{
		//Log.Info("Add " + newCapPoints + " new cap points to " + capacityPoints);
		if (capacityPoints + newCapPoints > 0 && capacityPoints + newCapPoints <= CapacityCap)
		{
			capacityPoints += newCapPoints;
		}
	}

	public void addUnit(GameObject newUnit, int unitCapPts)
	{
		//Log.Info("Add Unit");
		myUnits.Add( newUnit );
		addCapacityPoints(unitCapPts);
	}

	public void removeUnit(GameObject unitToRemove, int unitCapPts)
	{
		//Log.Info("Remove Unit");
		myUnits.Remove( unitToRemove );
		addCapacityPoints( -unitCapPts );
	}
}
