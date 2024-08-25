using Sandbox.Citizen;
using Sandbox.UI;
using Sandbox.UI.Construct;
using Sandbox.Utility.Svg;
using System;
using System.Linq;
public class SpectatorControl : Component
{

	[Property]	RTSCamComponent RTSCam {  get; set; }

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
