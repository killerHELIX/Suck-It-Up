using System;
public class SpawnerOrb : ControlOrb
{
	[Property] List<UnitPortraitTuple> spawnableUnitList {  get; set; }
	protected override void OnStart()
	{
		base.OnStart();
		if (GameState.Local.getPlayerTypeFromPlayer(Connection.Local.DisplayName) != GameState.PlayerType.RTS)
		{
			Log.Info("Fucking hello?");
			//PhysicalModelRenderer.skinnedModel.Enabled = false;
			//PhysicalModelRenderer.Enabled = false;
			//OrbHighlight.Enabled = false;

			Enabled = false;
			return;
		}
		ThisOrbType = OrbType.Spawner;
		foreach(var unitType in spawnableUnitList) 
		{
			buttons.Add(new ConstructUnitButton('.', unitType.UnitPortraitImage, unitType.UnitPrefab, Transform.Position, unitType.UnitResourceCost, unitType.UnitCapacityCost));
		}
	}
}
