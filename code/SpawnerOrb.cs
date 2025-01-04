using System;
public class SpawnerOrb : ControlOrb
{
	[Property] List<UnitPortraitTuple> spawnableUnitList {  get; set; }

	private int localCurretPhase = 0;
	private Vector3 groundPosition;

	protected override void OnStart()
	{
		base.OnStart();
		if (GameState.Local.getPlayerTypeFromPlayer(Connection.Local.DisplayName) != GameState.PlayerType.RTS)
		{
			Enabled = false;
			return;
		}
		ThisOrbType = OrbType.Spawner;

		var groundRay = Scene.Trace.Ray(new Ray(Transform.Position, Vector3.Down), 5000.0F);
		var tr = groundRay.Run();
		groundPosition = tr.EndPosition;

		foreach (var unitType in spawnableUnitList) 
		{
			if(unitType.PhaseUnlocked <= GameState.Local.matchPhase)
			{
				buttons.Add(new ConstructUnitButton('.', unitType.UnitPortraitImage, unitType.UnitPrefab, groundPosition, unitType.UnitResourceCost, unitType.UnitCapacityCost, unitType.UnitName));
			}
		}
	}

	protected override void OnUpdate()
	{
		if (iswaitingForPhase)
		{
			if (PhaseOrbUnlocked <= GameState.Local.matchPhase)
			{
				iswaitingForPhase = false;
				setNonInteractable(false);
			}
		}
		if(GameState.Local.matchPhase != localCurretPhase)
		{
			localCurretPhase = GameState.Local.matchPhase;
			buttons.Clear();
			foreach (var unitType in spawnableUnitList)
			{
				if (unitType.PhaseUnlocked <= GameState.Local.matchPhase)
				{
					buttons.Add(new ConstructUnitButton('.', unitType.UnitPortraitImage, unitType.UnitPrefab, Transform.Position, unitType.UnitResourceCost, unitType.UnitCapacityCost, unitType.UnitName));
				}
			}
		}
	}
}
