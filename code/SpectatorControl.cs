
public class SpectatorControl : Component
{

	[Property]	public RTSCamComponent RTSCam {  get; set; }

	protected override void OnStart()
	{
		if (Network.IsProxy)
		{
			Log.Info("Non-local spectator controller, disabling");
			RTSCam.Enabled = false;
			Enabled = false;
			return;
		}
		base.OnStart();
	}
}
