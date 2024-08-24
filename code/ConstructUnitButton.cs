using System;
public class ConstructUnitButton : DynamicButton
{

	public GameObject buttonUnitPrefab { get; set; }

	public Vector3 thisSpawnerPosition { get; set; }

	private int constructionCost = 0;

	private int capacityCost = 0;

	public ConstructUnitButton(char hotkey, string bg1, GameObject unitPrefab, Vector3 spawnerPosition, int cost, int capacity) : base()
	{
		hotkeyChar = hotkey;
		activeBackgroundImage = bg1;
		buttonUnitPrefab = unitPrefab;
		thisButtonAction = () => constructUnit(unitPrefab);
		thisSpawnerPosition = spawnerPosition;
		constructionCost = cost;
		capacityCost = capacity;
	}

	public void constructUnit(GameObject unitPrefab)
	{
		RTSPlayer.Local.myUnitFactory.spawnUnit(unitPrefab, RTSPlayer.Local.Team, thisSpawnerPosition);
		RTSPlayer.Local.resourcePoints = RTSPlayer.Local.resourcePoints - constructionCost;
	}

	public bool canAffordUnit()
	{
		if(RTSPlayer.Local.resourcePoints >= constructionCost && RTSPlayer.Local.CapacityCap - RTSPlayer.Local.capacityPoints >= capacityCost)
		{
			return true;
		}
		else
		{
			return false;
		}
	}

	public override string getIsDisabledStyle()
	{
		setEnabled(canAffordUnit());
		return isEnabledStyleString;
	}

	public override string getIsDisabledStyleHint()
	{
		setEnabled(canAffordUnit());
		return isEnabledStyleHintString;
	}

	public override string getPointerEventsStyle()
	{
		setEnabled(canAffordUnit());
		return buttonPointerEventsString;
	}
}
