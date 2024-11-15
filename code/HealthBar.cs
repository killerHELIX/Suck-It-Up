﻿using Sandbox;

public class HealthBar : Component
{

	[Property] public WorldPanel UnitStatusWorldPanel;
	[Property] public HealthBarUI healthBarUI;

	private const float WIDTH_MULTIPLIER = 50f;
	private const float SET_HEIGHT = 10f;
	public void setHealth(int currentHealth, int maxHealth)
	{
		healthBarUI.setHealth((int)(((float)currentHealth/(float)maxHealth) * 100));
		healthBarUI.StateHasChanged();
	}

	public void setShowHealthBar(bool showBar)
	{
		healthBarUI.Enabled = showBar;
		healthBarUI.StateHasChanged();
	}

	public void setBarColor(string newColor)
	{
		healthBarUI.setBarColor(newColor);
		healthBarUI.StateHasChanged();
	}

	public void setSize(Vector3 size)
	{
		float targetWidth = float.Min(WIDTH_MULTIPLIER * float.Min( size.x, size.y ), 500f);
		float targetHeight = SET_HEIGHT;
		UnitStatusWorldPanel.PanelSize = new Vector2( targetWidth, targetHeight );
	}

	public void setEnabled(bool newEnabled)
	{
		healthBarUI.Enabled = newEnabled;
		UnitStatusWorldPanel.Enabled = newEnabled;
	}
	/*protected override void OnDestroy()
	{
		UnitStatusWorldPanel.Enabled = false;
		UnitStatusWorldPanel.Destroy();
		healthBarUI.Enabled = false;
		healthBarUI.Destroy();
		base.OnDestroy();
	}*/
}
