using Sandbox;
using Sandbox.Citizen;
using System;
using System.Drawing;
using System.Runtime;

public class FPSPlayerController : Component
{
	[Property] CameraComponent Head;
	[Property] GameObject Body;

	[Property] float MovementSpeed { get; set; } = 100f;

	public Angles EyeAngles { get; set; }

    protected override void OnEnabled()
    {
        base.OnEnabled();

		if ( Head is not null )
		{
			var ee = Head.Transform.Rotation.Angles();
			ee.roll = 0;
			EyeAngles = ee;
		}
    }

    protected override void OnUpdate()
	{

		// Player Position moves based on AnalogMove input.
		Transform.Position += Input.AnalogMove * MovementSpeed * Time.Delta;

		// Starting from EyeAngles, rotate the Head and Body. 
		// Head Pitch Clamped, 0 Roll.
		// Body 0 Pitch, 0 Roll (Only Yaw).
		var ee = EyeAngles;
		ee += Input.AnalogLook;
		ee.pitch = ee.pitch.Clamp(-90, 90);
		ee.roll = 0;
		EyeAngles = ee;
		var bodyAngles = ee.WithPitch(0);

		Head.Transform.Rotation = EyeAngles.ToRotation();
		Body.Transform.Rotation = bodyAngles.ToRotation();
	}

    private void UpdateHeadRotation()
    {
    }

    private void UpdateBodyPosition()
	{
	}
}