using System;

public class DynamicMultiStateButton : DynamicButton
{
	public int buttonState { get; set; }
	public List<string> stateList {get; set; } = new List<string>();

	public DynamicMultiStateButton(char hotkey, List<string>bgStateList, Action clickAction, string thisHoverHint) : base(hotkey, bgStateList[0], clickAction, thisHoverHint)
	{
		buttonState = 0;
		stateList = bgStateList;
	}

	public void setButtonState(int desiredBG)
	{
		if (desiredBG >= 0 && desiredBG < stateList.Count)
		{
			buttonState = desiredBG;
			activeBackgroundImage = stateList[desiredBG];
		}
		else
		{
			Log.Error(desiredBG + " is not a legal Button State!");
		}
	}

	public void toggleButtonState()
	{
		buttonState++;
		if(buttonState == stateList.Count)
		{
			buttonState = 0;
		}
		activeBackgroundImage = stateList[buttonState];
	}

}
