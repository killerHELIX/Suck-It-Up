using Sandbox;
using System;

class SIUUnit : Unit
{

	[Property] public float IndividualModelScale { get; set; }
	[Property] public Vector3 localEyeBallPosition { get; set; }
	[Property] public ModelHitboxes myHitBoxes { get; set; }

	// This will be a factor of the unit size I imagine
	private float lastMeleeTime = Time.Now;
	private Vector3 oldPosition = new Vector3();
	private float stuckTime = -1;

	private DynamicToggleButton unitStanceButton;

	private bool canSeeTempTarget = false;

	// Unit Constants
	private const float AUTO_MELEE_RAD_DIST = 650f;
	private const float NAV_AGENT_RAD_MULTIPLIER = .5f;
	private const float CLICK_HITBOX_RADIUS_MULTIPLIER = .5f;
	private const float GLOBAL_UNIT_SCALE = 15f;
	private const float BASE_STAND_SIZE_MULTIPLIER = 1.6f;

	// Nav Constant
	private const float MAX_STUCK_TIME = 5f;
	private const float CLOSE_ENOUGH_TIME = .5f;
	private const float CLOSE_ENOUGH_DISTANCE = .5f;
	private const float STUCK_DISTANCE = .1f;

	private const string PLAYER_TAG = "player";

	private const string AttackStanceImagePath = "materials/attack_stance.png";
	private const string DefendStanceImagePath = "materials/defend_stance.png";

	protected override void OnUpdate()
	{
		if (!Network.IsOwner) { 
			if(Network.OwnerConnection == null)
			{
				Network.TakeOwnership();
			}
			else
			{
				return;
			} 
		}
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
					oldPosition = Transform.Position;
					stuckTime = -1;
					move(homeTargetLocation, true);
				}
			}
			else
			{
				//Log.Info(Transform.Position.Distance(homeTargetLocation) + ", " + oldPosition.Distance(Transform.Position) + ", " + (Time.Now - stuckTime));
				if (oldPosition.Distance(Transform.Position) >= STUCK_DISTANCE)
				{
					oldPosition = Transform.Position;
				}
				// We are probably stuck
				else
				{
					if(stuckTime < 0)
					{
						stuckTime = Time.Now;
					}
					// If stuck for 3 seconds but close to target, consider yourself there
					else if (Time.Now - stuckTime > CLOSE_ENOUGH_TIME && Transform.Position.Distance(homeTargetLocation) < CLOSE_ENOUGH_DISTANCE)
					{
						stuckTime = -1;
						//Log.Info("Made it Vaguely to the correct spot");
						commandGiven = UnitModelUtils.CommandType.None;
						stopMoving();
					}
					// Give up after being stuck for 5 seconds
					else if(Time.Now - stuckTime > MAX_STUCK_TIME)
					{
						stuckTime = -1;
						//Log.Info("Unit stuck, stopping");
						commandGiven = UnitModelUtils.CommandType.None;
						stopMoving();
					}
				}
			}
			// These commands don't care whether they are new or old
			// Attack Command
			if (commandGiven == UnitModelUtils.CommandType.Attack)
			{
				if (targetObject.Components.Get<FPSHealthController>().IsAlive == true)
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
		else if (tempTargetObject != null && tempTargetObject.Components.Get<FPSHealthController>().IsAlive)
		{
			//Log.Info("Chasing seen unit " + this.GameObject);
			homeTargetLocation = tempTargetObject.Transform.Position;
			move(homeTargetLocation, false);
		}
		else
		{
			//Log.Info("Lost unit or no target, continuing to last known location " + this.GameObject);
			//stopMoving();
		}

		// Handle Attacks
		// Attack Unit in melee range
		if (UnitMeleeCollider != null)
		{
			// Get touching trigger colliders
			var collidersInMeleeRange = UnitMeleeCollider.Touching;
			// Select colliders belonging to Units
			if (collidersInMeleeRange.Where(col => col.Tags.Has(PLAYER_TAG)).Any())
			{
				// Select only melee colliders
				foreach (var collision in collidersInMeleeRange.Where(col => col.Tags.Has(PLAYER_TAG)))
				{
					if (Time.Now - lastMeleeTime > MeleeAttackSpeed)
					{
						Log.Info( this.GameObject.Name + " attacks " + collision.GameObject.Name + " for " + MeleeAttackDamage + " damage!" );
						directMeleeAttack(collision.GameObject);
					}
				}
			}
		}
		// Basic Auto tracking for seen players. Only do this when idle
		if (UnitAutoMeleeCollider != null && isInAttackMode && commandGiven==UnitModelUtils.CommandType.None)
		{
			var validPlayerFound = false;
			//Log.Info("In auto melee collider");

			// We have no current target
			if (tempTargetObject == null)
			{
				// Get touching trigger colliders
				var collidersInAutoMeleeRange = UnitAutoMeleeCollider.Touching;
				//Log.Info("Colliders in range " + collidersInAutoMeleeRange.Count());
				// Select colliders belonging to Units
				if (collidersInAutoMeleeRange.Where(col => col.Tags.Has(PLAYER_TAG)).Any())
				{
					//Log.Info("Player Colliders in range " + collidersInAutoMeleeRange.Where(col => col.Tags.Has(PLAYER_TAG)).Count());
					// Select only melee colliders
					foreach (var collision in collidersInAutoMeleeRange.Where(col => col.Tags.Has(PLAYER_TAG)))
					{
						var playerCollidedWith = collision.GameObject;
						//Log.Info("Detected Player: " + playerCollidedWith.Name + ". Can I see him?");
						//Log.Info("Ray from " + Transform.Position + localEyeBallPosition + "to " + playerCollidedWith.Transform.Position);
						// Draw a ray here to detect whether or not we can see the unit
						var sightRay = Scene.Trace.Ray(Transform.Position + localEyeBallPosition, playerCollidedWith.Transform.Position);
						//sightRay.UseHitboxes(true);
						//sightRay.HitTriggers();
						sightRay.WithTag(PLAYER_TAG);
						var sightRayTrace = sightRay.RunAll();
						if (sightRayTrace.Any())
						{
							//foreach (var hit in sightRayTrace)
							//{
								//Log.Info("Hit: " + hit.GameObject);
							//}
							// I can see it
							if (sightRayTrace.First().GameObject == playerCollidedWith)
							{
								//Log.Info( this.GameObject.Name + " will attack " + playerCollidedWith.Name + "!" );
								validPlayerFound = true;
								canSeeTempTarget = true;
								tempTargetObject = playerCollidedWith;
							}
						}
						else
						{
							//Log.Info("Can't see anything");
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
				//Log.Info(sightRayTrace.Count());
				if (sightRayTrace.Any())
				{
					// I can see it
					if (sightRayTrace.First().GameObject != tempTargetObject)
					{
						//Log.Info( this.GameObject.Name + " will cease attacking " + tempTargetObject.Name + "!" );
						removeTempTarget = true;
					}
				}
				if (removeTempTarget)
				{
					//Log.Info("Removing Temp Target");
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
		Log.Info( this.GameObject.Name + " dies!" );
		if(RTSPlayer.Local != null)
		{
			RTSPlayer.Local.UnitControl.unitHasDied(this);
		}
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
		myHitBoxes.Enabled = false;
		if (RTSPlayer.Local != null)
		{
			RTSPlayer.Local.removeUnit(this.GameObject, CapacityCost);
		}
		Enabled = false;

		//This will be fully destroyed later when the corpse dissapears
		PhysicalModelRenderer.addToCorpsePile();
	}

	public void takeDamage(int damage, GameObject source)
	{
		//Log.Info( this.GameObject.Name + " takes " + damage + " damage!");
		PhysicalModelRenderer.animateDamageTaken();
		setHealth(currentHealthPoints - damage);
		if (currentHealthPoints <= 0)
		{
			die();
		}
		else
		{
			if(tempTargetObject == null && targetObject == null)
			{
				Log.Info(GameObject.Name + ": I've been shot! Fuck you, " + source.Name);
				tempTargetObject = source;
				homeTargetLocation = source.Transform.Position;
			}
		}
	}

	[Broadcast]
	private void directMeleeAttack(GameObject targetPlayer)
	{
		this.PhysicalModelRenderer.animateMeleeAttack();
		targetPlayer.Components.Get<FPSHealthController>().Damage(MeleeAttackDamage);
		lastMeleeTime = Time.Now;
	}

	public override void setRelativeSizeHelper(Vector3 unitSize)
	{
		// The scale is going to be calculated from the ratio of the default model size and the unit's given size modified by a global scaling constant
		Vector3 defaultModelSize = ModelFile.Bounds.Size;

		//Vector3 globalScaleModifier = Vector3.One * Scene.GetAllObjects( true ).Where( go => go.Name == "RTSGameOptions" ).First().Components.GetAll<RTSGameOptionsComponent>().First().getFloatValue( RTSGameOptionsComponent.GLOBAL_UNIT_SCALE );
		//Log.Info( ModelFile.Bounds.Size );

		Vector3 globalScaleModifier = Vector3.One * GLOBAL_UNIT_SCALE * IndividualModelScale ;//RTSPlayer.Local.LocalGame.GameOptions.getFloatValue( RTSGameOptionsComponent.GLOBAL_UNIT_SCALE );
		Vector3 targetModelSize = new Vector3((unitSize.x * globalScaleModifier.x), (unitSize.y * globalScaleModifier.y), (unitSize.z * globalScaleModifier.z));
		float targetxyMin = float.Min(targetModelSize.x, targetModelSize.y);
		float targetxyMax = float.Max(targetModelSize.x, targetModelSize.y);
		float defaultxyMin = float.Min(defaultModelSize.x, defaultModelSize.y);
		float defaultxyMax = float.Max(defaultModelSize.x, defaultModelSize.y);
		//Log.Info("global modifier" + globalScaleModifier);
		//Log.Info("defaultModelSize: " +  defaultModelSize);
		//Log.Info("Target Model Size: " + targetModelSize );
		//Log.Info( "Calculated Scale: " + new Vector3(
		//((unitSize.x * globalScaleModifier.x) / defaultModelSize.x),
		//((unitSize.y * globalScaleModifier.y) / defaultModelSize.y),
		//((unitSize.z * globalScaleModifier.z) / defaultModelSize.z)
		//));
		/*Transform.LocalScale = new Vector3(
			(targetModelSize.x / defaultModelSize.x),
			(targetModelSize.y / defaultModelSize.y),
			(targetModelSize.z / defaultModelSize.z)
			);*/

		// Auto calculate unit's nav agent size
		UnitNavAgent.Height = targetModelSize.z;
		UnitNavAgent.Radius = targetxyMin * NAV_AGENT_RAD_MULTIPLIER;

		// Auto calculate unit's melee collider size
		UnitMeleeCollider.Radius = defaultxyMax;
		UnitMeleeCollider.Start = Vector3.Zero;
		UnitMeleeCollider.End = new Vector3(0, 0, defaultModelSize.z);

		// Auto calculate unit's auto melee collider size
		UnitAutoMeleeCollider.Center = Vector3.Zero;
		UnitAutoMeleeCollider.Radius = AUTO_MELEE_RAD_DIST + targetxyMin;

		// Auto calculate unit's ranged attack range collider size


		// Auto calculate unit's Selection Collider scaling and relative position
		SelectionHitbox.Center = new Vector3(0, 0, defaultModelSize.z / 2);
		SelectionHitbox.Scale = new Vector3(defaultxyMin * CLICK_HITBOX_RADIUS_MULTIPLIER, defaultxyMin * CLICK_HITBOX_RADIUS_MULTIPLIER, defaultModelSize.z);

		// Auto Calculate other visual element sizes
		PhysicalModelRenderer.setModelSize(BASE_STAND_SIZE_MULTIPLIER * defaultModelSize);
		ThisHealthBar.setSize(defaultModelSize);
	}
}
