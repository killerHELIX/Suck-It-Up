using Sandbox.Network;
using System.Threading;

public class StaticSUIGameTrigger : Component, Component.ITriggerListener
{

	[Property] public Collider TriggerCollider { get; set; }
	//[Property] public Trigg EndGamePanel { get; set; }
	[Property] public int GamePhase { get; set; }
	[Property] public bool GameFinish { get; set; }


	public void onTriggerEnter(Collider other)
	{
		Log.Info("Trigger Entered");
		if(other.GameObject.Tags.Contains("player"))
		{
			Log.Info("Player Entered Trigger");
			if (GameFinish)
			{
				GameState.Local.EndGamePanel.survivorsWon = true;
				GameState.Local.EndGamePanel.StateHasChanged();
				GameState.Local.finishGame();
				return;
			}
			if(GamePhase > GameState.Local.matchPhase)
			{
				GameState.Local.setPhase(GamePhase);
			}
		}
	}

	protected override void OnUpdate()
	{
		var touchingCols = TriggerCollider.Touching;
		if (touchingCols != null)
		{

			foreach (Collider col in touchingCols)
			{
				if (col.GameObject.Tags.Contains("player"))
				{
					if (GameFinish)
					{
						GameState.Local.finishGame();
						return;
					}
					if (GamePhase > GameState.Local.matchPhase)
					{
						GameState.Local.setPhase(GamePhase);
						return;
					}
				}
			}
		}
	}

	public void onTriggerExit(Collider other)
	{
		//noop
	}
}
