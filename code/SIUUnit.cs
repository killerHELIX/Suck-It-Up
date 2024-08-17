using Sandbox;
using System;

class SIUUnit : Unit
{

	// This will be a factor of the unit size I imagine
	private float maxChaseDistanceFromHome = 600f;
	private float lastMeleeTime = Time.Now;
	private float lastMoveOrderTime = Time.Now;

	private DynamicToggleButton unitStanceButton;

	private bool canSeeTempTarget = false;

	// Unit Constants
	private const float MOVE_ORDER_FREQUENCY = .1f;
	private const float CHASE_DIST_MULTIPLIER = 90f;
	private const float AUTO_MELEE_RAD_MULTIPLIER = 90f;
	private const float NAV_AGENT_RAD_MULTIPLIER = .5f;
	private const float CLICK_HITBOX_RADIUS_MULTIPLIER = .5f;
	private const float GLOBAL_UNIT_SCALE = .1f;

	private const string AttackStanceImagePath = "materials/attack_stance.png";
	private const string DefendStanceImagePath = "materials/defend_stance.png";

	protected override void OnUpdate()
	{
		if (!Network.IsOwner) { return; }
		// Handle Player Commands
		if (commandGiven != UnitModelUtils.CommandType.None)
		{
			// Command just came in
			if (isNewCommand)
			{
				// Age the command
				isNewCommand = false;
				// Move Command
				if (commandGiven == UnitModelUtils.CommandType.Move)
				{
					move(homeTargetLocation, true);
				}
			}
			else
			{
				if(Transform.Position.Distance(homeTargetLocation) < 3)
				{
					commandGiven = UnitModelUtils.CommandType.None;
				}
			}
			// These commands don't care whether they are new or old
			// Attack Command
			if (commandGiven == UnitModelUtils.CommandType.Attack)
			{
				if (targetObject.isAlive == true)
				{
					move(targetObject.Transform.Position, true);
				}
				//Reset if unit is killed or deleted or something
				else
				{
					homeTargetLocation = Transform.Position;
					commandGiven = UnitModelUtils.CommandType.None;
					stopMoving();
				}
			}

		}
		// Move To closeby enemy that we can see
		else if (tempTargetObject != null && tempTargetObject.isAlive)
		{
			//Log.Info("Chasing seen unit " + this.GameObject);
			homeTargetLocation = tempTargetObject.Transform.Position;
			move(homeTargetLocation, false);
		}
		else
		{
			//Log.Info("Lost unit, continuing to last known location " + this.GameObject);
			//stopMoving();
		}

		// Handle Attacks
		// Attack Unit in melee range
		if (UnitMeleeCollider != null)
		{
			// Get touching trigger colliders
			var collidersInMeleeRange = UnitMeleeCollider.Touching;
			// Select colliders belonging to Units
			if (collidersInMeleeRange.Where(col => (col.Tags.Has(UNIT_TAG) || col.Tags.Has(Building.BUILDING_TAG))).Any())
			{
				// Select only melee colliders
				foreach (var collision in collidersInMeleeRange.Where(col => (col.Tags.Has(UNIT_TAG) || col.Tags.Has(Building.BUILDING_TAG))))
				{
					//Collider belongs to unit
					if (collision.Tags.Has(UNIT_TAG))
					{
						var unitCollidedWith = collision.GameObject.Components.GetAll().OfType<Unit>().First();
						if (collision == unitCollidedWith.UnitMeleeCollider && unitCollidedWith.team != team)
						{
							if (Time.Now - lastMeleeTime > MeleeAttackSpeed)
							{
								//Log.Info( this.GameObject.Name + " attacks " + collision.GameObject.Name + " for " + MeleeAttackDamage + " damage!" );
								directMeleeAttack(unitCollidedWith);
							}
						}
					}
				}
			}
		}
		// Basic Auto tracking for seen players. Only do this when idle
		if (UnitAutoMeleeCollider != null && isInAttackMode && commandGiven==UnitModelUtils.CommandType.None)
		{
			var validPlayerFound = false;

			// We have no current target
			if (tempTargetObject == null)
			{
				// Get touching trigger colliders
				var collidersInAutoMeleeRange = UnitAutoMeleeCollider.Touching;
				// Select colliders belonging to Units
				if (collidersInAutoMeleeRange.Where(col => col.Tags.Has(UNIT_TAG)).Any())
				{
					// Select only melee colliders
					foreach (var collision in collidersInAutoMeleeRange)
					{
						var unitCollidedWith = collision.GameObject.Components.GetAll().OfType<Unit>().First();
						// If it is a unit of the opposite team
						if (unitCollidedWith.team != team)
						{
							// Draw a ray here to detect whether or not we can see the unit
							var sightRay = Scene.Trace.Ray(Transform.Position, unitCollidedWith.Transform.Position);
							//sightRay.UseHitboxes(true);
							var sightRayTrace = sightRay.RunAll();
							if (sightRayTrace.Any())
							{
								foreach (var hit in sightRayTrace)
								{
									Log.Info(hit.GameObject);
								}
								// I can see it
								if (sightRayTrace.First().GameObject == unitCollidedWith.GameObject)
								{
									//Log.Info( this.GameObject.Name + " will attack " + unitCollidedWith.GameObject.Name + "!" );
									validPlayerFound = true;
									canSeeTempTarget = true;
									tempTargetObject = unitCollidedWith;
								}
							}
						}
					}
				}
			}
			// Check that we can still see our target and that they are still within range
			else
			{
				var removeTempTarget = false;
				if(Transform.Position.Distance(tempTargetObject.Transform.Position) > UnitAutoMeleeCollider.Radius)
				{
					removeTempTarget = true;
				}

				// Draw a ray here to detect whether or not we can see the unit
				var sightRay = Scene.Trace.Ray(Transform.Position, tempTargetObject.Transform.Position);
				sightRay.UseHitboxes(true);
				var sightRayTrace = sightRay.RunAll();
				Log.Info(sightRayTrace.Count());
				if (sightRayTrace.Any())
				{
					// I can see it
					if (sightRayTrace.First().GameObject != tempTargetObject.GameObject)
					{
						//Log.Info( this.GameObject.Name + " will cease attacking " + tempTargetObject.GameObject.Name + "!" );
						removeTempTarget = true;
					}
				}
				if (removeTempTarget)
				{
					tempTargetObject = null;
				}
			}
			// Reset automelee if nothing in range
			//if (validPlayerFound != true)
			//{
			//	tempTargetObject = null;
			//}
		}

		// Handle Animations
		if (PhysicalModelRenderer != null && UnitNavAgent != null)
		{
			if (!UnitNavAgent.Velocity.IsNearZeroLength)
			{
				PhysicalModelRenderer.animateMovement(UnitNavAgent.Velocity, UnitNavAgent.WishVelocity);
			}
			else
			{
				PhysicalModelRenderer.stopMovementAnimate();
			}
		}
	}

	[Broadcast]
	public override void die()
	{
		//Log.Info( this.GameObject.Name + " dies!" );
		PhysicalModelRenderer.animateDeath();
		UnitNavAgent.Enabled = false;
		UnitMeleeCollider.Enabled = false;
		UnitAutoMeleeCollider.Enabled = false;
		if (UnitRangedAttackCollider != null)
		{
			UnitRangedAttackCollider.Enabled = false;
		}
		SelectionHitbox.Enabled = false;
		ThisHealthBar.Enabled = false;
		ThisHealthBar.setEnabled(false);
		PhysicalModelRenderer.baseStand.setEnabled(false);
		Enabled = false;

		//This will be fully destroyed later when the corpse dissapears
		PhysicalModelRenderer.addToCorpsePile();
	}

	public void move(Vector3 location, bool isNewMoveCommand)
	{
		if (!Network.IsOwner) { return; }
		if (location != UnitNavAgent.TargetPosition)
		{
			if (isNewMoveCommand || Time.Now - lastMoveOrderTime >= MOVE_ORDER_FREQUENCY)
			{
				lastMoveOrderTime = Time.Now;
				//Log.Info( "Move Command Sent: " + UnitNavAgent.TargetPosition );
				UnitNavAgent.MoveTo(location);
			}
		}
	}

	public void stopMoving()
	{
		if (!Network.IsOwner) { return; }
		homeTargetLocation = Transform.Position;
		UnitNavAgent.Stop();
	}

	[Broadcast]
	private void directMeleeAttack(SkinnedRTSObject targetUnit)
	{
		this.PhysicalModelRenderer.animateMeleeAttack();
		targetUnit.takeDamage(MeleeAttackDamage);
		lastMeleeTime = Time.Now;
	}

	public override void setRelativeSizeHelper(Vector3 unitSize)
	{
		// The scale is going to be calculated from the ratio of the default model size and the unit's given size modified by a global scaling constant
		Vector3 defaultModelSize = ModelFile.Bounds.Size;

		//Vector3 globalScaleModifier = Vector3.One * Scene.GetAllObjects( true ).Where( go => go.Name == "RTSGameOptions" ).First().Components.GetAll<RTSGameOptionsComponent>().First().getFloatValue( RTSGameOptionsComponent.GLOBAL_UNIT_SCALE );
		//Log.Info( ModelFile.Bounds.Size );

		Vector3 globalScaleModifier = Vector3.One * GLOBAL_UNIT_SCALE;//RTSPlayer.Local.LocalGame.GameOptions.getFloatValue( RTSGameOptionsComponent.GLOBAL_UNIT_SCALE );
		Vector3 targetModelSize = new Vector3((unitSize.x * globalScaleModifier.x), (unitSize.y * globalScaleModifier.y), (unitSize.z * globalScaleModifier.z));
		float targetxyMin = float.Min(targetModelSize.x, targetModelSize.y);
		float targetxyMax = float.Max(targetModelSize.x, targetModelSize.y);
		float defaultxyMin = float.Min(defaultModelSize.x, defaultModelSize.y);
		float defaultxyMax = float.Max(defaultModelSize.x, defaultModelSize.y);
		//Log.Info("defaultModelSize: " +  defaultModelSize);
		//Log.Info("Target Model Size: " + targetModelSize );
		//Log.Info( "Calculated Scale: " + new Vector3(
		//((unitSize.x * globalScaleModifier.x) / defaultModelSize.x),
		//((unitSize.y * globalScaleModifier.y) / defaultModelSize.y),
		//((unitSize.z * globalScaleModifier.z) / defaultModelSize.z)
		//));
		Transform.LocalScale = new Vector3(
			(targetModelSize.x / defaultModelSize.x),
			(targetModelSize.y / defaultModelSize.y),
			(targetModelSize.z / defaultModelSize.z)
			);

		// Auto calculate unit's nav agent size
		UnitNavAgent.Height = targetModelSize.z;
		UnitNavAgent.Radius = targetxyMin * NAV_AGENT_RAD_MULTIPLIER;

		// Auto calculate unit's melee collider size
		UnitMeleeCollider.Radius = defaultxyMax;
		UnitMeleeCollider.Start = Vector3.Zero;
		UnitMeleeCollider.End = new Vector3(0, 0, defaultModelSize.z);

		// Auto calculate unit's auto melee collider size
		UnitAutoMeleeCollider.Center = Vector3.Zero;
		UnitAutoMeleeCollider.Radius = AUTO_MELEE_RAD_MULTIPLIER * targetxyMax;

		// Auto calculate unit's ranged attack range collider size


		// Auto calculate unit's Selection Collider scaling and relative position
		SelectionHitbox.Center = new Vector3(0, 0, defaultModelSize.z / 2);
		SelectionHitbox.Scale = new Vector3(defaultxyMin * CLICK_HITBOX_RADIUS_MULTIPLIER, defaultxyMin * CLICK_HITBOX_RADIUS_MULTIPLIER, defaultModelSize.z);

		// Auto calculate unit's chase distance
		maxChaseDistanceFromHome = CHASE_DIST_MULTIPLIER * targetxyMax;

		// Auto Calculate other visual element sizes
		PhysicalModelRenderer.setModelSize(defaultModelSize);
		ThisHealthBar.setSize(defaultModelSize);
	}
}
