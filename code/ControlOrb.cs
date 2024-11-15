﻿public class ControlOrb : SelectableObject, IScalable, ISelectable
{
	[Group("Gameplay")]
	[Property] public OrbType ThisOrbType { get; set; }

	[Property] public int PhaseOrbUnlocked { get; set; }

	[Group("Visuals")]
	[Property] public HighlightOutline OrbHighlight { get; set; }

	// Class Vars
	//public int team = 0;
	bool selected { get; set; }

	protected bool iswaitingForPhase = true;

	System.Guid oldOwner { get; set; }

	// Constants
	private const float CLICK_HITBOX_RADIUS_MULTIPLIER = 1f;

	public const uint COLOR_SELECTED = 0x8000FFFF;
	public const uint COLOR_SPAWNER = 0x800000FF;
	public const uint COLOR_TRAP = 0x80005EFF;

	public enum OrbType
	{
		Spawner,
		Trap
	}

	protected override void OnStart()
	{
		objectTypeTag = "control_orb";
		base.OnStart();
		setDefaultColor();
		setTeam(0);
		Tags.Add(objectTypeTag);
		setNonInteractable(true);
	}

	protected override void OnUpdate()
	{
		if(iswaitingForPhase)
		{
			if(PhaseOrbUnlocked <= GameState.Local.matchPhase)
			{
				iswaitingForPhase=false;
				setNonInteractable(false);
			}
		}
	}

	public override void select()
	{
		if (!Network.IsOwner) { return; }
		selected = true;
		var spawnerColor = new Color(COLOR_SELECTED);
		PhysicalModelRenderer.skinnedModel.Tint = spawnerColor;
		OrbHighlight.InsideObscuredColor = spawnerColor;
		OrbHighlight.ObscuredColor = spawnerColor;
	}

	public override void deSelect()
	{
		if (!Network.IsOwner) { return; }
		selected = false;
		setDefaultColor();
	}

	protected void setNonInteractable(bool isNonInteractable)
	{
		if(isNonInteractable)
		{
			if(PhysicalModelRenderer != null)
			{
				this.PhysicalModelRenderer.skinnedModel.Enabled = false;
				this.PhysicalModelRenderer.Enabled = false;
			}
			this.SelectionHitbox.Enabled = false;
			this.OrbHighlight.Enabled = false;
		}
		else
		{
			if (PhysicalModelRenderer != null)
			{
				this.PhysicalModelRenderer.skinnedModel.Enabled = true;
				this.PhysicalModelRenderer.Enabled = true;
			}
			this.SelectionHitbox.Enabled = true;
			this.OrbHighlight.Enabled = true;
		}

	}

	public void onOwnerJoin()
	{
		Log.Info("OnOwnerJoin");
		if (!(team==RTSPlayer.Local.Team))
		{
			Log.Info("Not on my team");
			setNonInteractable(true);
		}
		else
		{
			if (PhaseOrbUnlocked == 0)
			{
				Log.Info("starter orb");
				setNonInteractable(false);
				iswaitingForPhase = false;
				setDefaultColor();
			}
			else
			{
				Log.Info("non starter orb");
				iswaitingForPhase = true;
			}
		}
	}

	private void setDefaultColor()
	{
		if(PhysicalModelRenderer != null)
		{
			if (ThisOrbType == OrbType.Spawner)
			{
				var spawnerColor = new Color(COLOR_SPAWNER);
				PhysicalModelRenderer.skinnedModel.Tint = spawnerColor;
				OrbHighlight.InsideObscuredColor = spawnerColor;
				OrbHighlight.ObscuredColor = spawnerColor;
			}
			else
			{
				var spawnerColor = new Color(COLOR_TRAP);
				PhysicalModelRenderer.skinnedModel.Tint = spawnerColor;
				OrbHighlight.InsideObscuredColor = spawnerColor;
				OrbHighlight.ObscuredColor = spawnerColor;
			}
		}
	}

	public override void setRelativeSizeHelper(Vector3 unitSize)
	{
		Log.Info(unitSize);
		Log.Info(ModelFile);
		Log.Info(ModelFile.Bounds);
		Log.Info(ModelFile.Bounds.Size);
		// The scale is going to be calculated from the ratio of the default model size and the object's given size modified by a global scaling constant
		Vector3 defaultModelSize = ModelFile.Bounds.Size;

		//Vector3 globalScaleModifier = Vector3.One * Scene.GetAllObjects(true).Where(go => go.Name == "RTSGameOptions").First().Components.GetAll<RTSGameOptionsComponent>().First().getFloatValue(RTSGameOptionsComponent.GLOBAL_UNIT_SCALE);
		Vector3 targetModelSize = new Vector3((unitSize.x), (unitSize.y), (unitSize.z));
		float targetxyMin = float.Min(targetModelSize.x, targetModelSize.y);
		float targetxyMax = float.Max(targetModelSize.x, targetModelSize.y);
		float defaultxyMin = float.Min(defaultModelSize.x, defaultModelSize.y);
		float defaultxyMax = float.Max(defaultModelSize.x, defaultModelSize.y);

		Transform.LocalScale = new Vector3(
			(targetModelSize.x / defaultModelSize.x),
			(targetModelSize.y / defaultModelSize.y),
			(targetModelSize.z / defaultModelSize.z)
			);

		// Auto calculate object's Selection Collider scaling and relative position
		SelectionHitbox.Center = new Vector3(0, 0, 0);
		SelectionHitbox.Scale = new Vector3(defaultxyMin * CLICK_HITBOX_RADIUS_MULTIPLIER, defaultxyMin * CLICK_HITBOX_RADIUS_MULTIPLIER, defaultModelSize.z);

		// Auto Calculate other visual element sizes
		PhysicalModelRenderer.setModelSize(defaultModelSize);
	}
}
