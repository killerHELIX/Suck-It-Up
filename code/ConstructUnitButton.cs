using System;
public class ConstructUnitButton : DynamicButton
{

	public GameObject buttonUnitPrefab { get; set; }

	public Vector3 thisSpawnerPosition { get; set; }

	private uint constructionCost = 0;

	public ConstructUnitButton(char hotkey, string bg1, GameObject unitPrefab, Vector3 spawnerPosition, uint cost) : base()
	{
		hotkeyChar = hotkey;
		activeBackgroundImage = bg1;
		buttonUnitPrefab = unitPrefab;
		thisButtonAction = () => constructUnit(unitPrefab);
		thisSpawnerPosition = spawnerPosition;
		constructionCost = cost;
	}

	public void constructUnit(GameObject unitPrefab)
	{
		RTSPlayer.Local.myUnitFactory.spawnUnit(unitPrefab, RTSPlayer.Local.Team, thisSpawnerPosition);
		RTSPlayer.Local.resourcePoints = RTSPlayer.Local.resourcePoints - constructionCost;
	}

	public bool canAffordUnit()
	{
		if(RTSPlayer.Local.resourcePoints >= constructionCost)
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
