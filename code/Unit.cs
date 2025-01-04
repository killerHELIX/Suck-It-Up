using Sandbox;
using System;

public class Unit : SkinnedRTSObject
{
	[Group( "Gameplay" )]
	[Property] public float UnitSpeed { get; set; }
	[Group( "Gameplay" )]
	[Property] public bool HasMeleeAttack { get; set; }
	[Group( "Gameplay" )]
	[Property] public int MeleeAttackDamage { get; set; }
	[Group( "Gameplay" )]
	[Property] public float MeleeAttackSpeed { get; set; }
	[Group( "Gameplay" )]
	[Property] public bool HasRangedAttack { get; set; }
	[Group( "Gameplay" )]
	[Property] public float RangedAttackRange { get; set; }
	[Group( "Gameplay" )]
	[Property] public int RangedAttackDamage { get; set; }
	[Group( "Gameplay" )]
	[Property] public float RangedAttackSpeed { get; set; }
	[Group("Gameplay")]
	[Property] public int ResourceCost { get; set; }
	[Group("Gameplay")]
	[Property] public int CapacityCost { get; set; }

	[Group( "Triggers And Collision" )]
	[Property] public UnitTriggerListener TriggerListener { get; set; }
	[Group( "Triggers And Collision" )]
	[Property] public CapsuleCollider UnitMeleeCollider { get; set; }
	[Group( "Triggers And Collision" )]
	[Property] public SphereCollider UnitAutoMeleeCollider { get; set; }
	[Group( "Triggers And Collision" )]
	[Property] public Collider UnitRangedAttackCollider { get; set; }
	[Group( "Triggers And Collision" )]
	[Property] public NavMeshAgent UnitNavAgent { get; set; }

	// Class Vars
	bool selected { get; set; }
	public UnitModelUtils.CommandType commandGiven { get; set; }
	public Vector3 homeTargetLocation { get; set; }
	public GameObject targetObject { get; set; }
	public GameObject tempTargetObject { get; set; }

	public enum Stance
	{
		Attack,
		Defend,
		Horde
	}

	// This will be a factor of the unit size I imagine
	private float maxChaseDistanceFromHome = 600f;
	private float lastMeleeTime = Time.Now;
	private float lastMoveOrderTime = Time.Now;
	public int currentStance = 0;
	protected bool hasReachedMoveTarget = true;
	protected bool isNewCommand = false;

	private DynamicMultiStateButton unitStanceButton;
	private DynamicButton recycleUnitButton;

	// Unit Constants
	public const string UNIT_TAG = "unit";
	private const float MOVE_ORDER_FREQUENCY = .1f;
	private const float CHASE_DIST_MULTIPLIER = 30f;
	private const float AUTO_MELEE_RAD_MULTIPLIER = 30f;
	private const float NAV_AGENT_RAD_MULTIPLIER = .5f;
	private const float CLICK_HITBOX_RADIUS_MULTIPLIER = .5f;
	private const float GLOBAL_UNIT_SCALE = .1f;

	// Stance Image References
	private const string AttackStanceImagePath = "materials/attack_stance.png";
	private const string DefendStanceImagePath = "materials/defend_stance.png";
	private const string HordeStanceImagePath = "materials/attack_closest.png";
	private const string RecycleImagepath = "materials/recycle_icon.png";

	public Dictionary<Stance, string> stanceMap = new Dictionary<Stance, string>();

	protected override void OnStart()
	{
		objectTypeTag = UNIT_TAG;
		base.OnStart();

		// Initialize stances
		stanceMap.Add(Stance.Attack, AttackStanceImagePath);
		stanceMap.Add(Stance.Defend, DefendStanceImagePath);
		stanceMap.Add(Stance.Horde, HordeStanceImagePath);
		var stanceList = new List<string>();
		foreach (var stance in Enum.GetValues<Stance>())
		{
			Log.Info((int)stance + " stance is path " + stanceMap[stance]);
			stanceList.Add(stanceMap[stance]);
		}
			
		// Init AI
		commandGiven = UnitModelUtils.CommandType.None;
		homeTargetLocation = Transform.Position;

		// Initialize UI elements
		unitStanceButton = new DynamicMultiStateButton('x', stanceList, stanceButtonClicked, "Unit Stance");
		recycleUnitButton = new DynamicButton('.', RecycleImagepath, recycleUnit, "Recycle Unit");
		buttons.Add(unitStanceButton);
		buttons.Add(recycleUnitButton);
		UnitNavAgent.MaxSpeed = UnitSpeed;
		UnitNavAgent.Acceleration = UnitSpeed * 10;
	}

	protected override void OnUpdate()
	{
		if (!Network.IsOwner) { return; }
		// Handle Player Commands
		if ( commandGiven != UnitModelUtils.CommandType.None )
		{
			// Attack Command
			if(commandGiven == UnitModelUtils.CommandType.Attack)
			{
				if(targetObject.Enabled == true )
				{
					move( targetObject.Transform.Position, false );
				}
				//Reset if unit is killed or deleted or something
				else
				{
					commandGiven = UnitModelUtils.CommandType.None;
					stopMoving();
				}
			}
			// Move Command
			else if (commandGiven == UnitModelUtils.CommandType.Move )
			{
				move( homeTargetLocation, true );
				commandGiven = UnitModelUtils.CommandType.None;
			}
		}
		// Move To closeby enemy
		else if( tempTargetObject != null)
		{
			//Log.Info( tempTargetUnit.GameObject.Name);
			//There is a bug here, somehow sometimes it gets in here between seeing that the enemy unit has become null
			if( tempTargetObject.Transform.Position.Distance( homeTargetLocation ) < maxChaseDistanceFromHome )
			{
				move( tempTargetObject.Transform.Position, false);
			}
			else
			{
				move( homeTargetLocation, false );
			}
		}
		else
		{
			move( homeTargetLocation, false );
		}

		// Handle Attacks
		// Attack Unit in melee range
		if ( UnitMeleeCollider != null ) 
		{
			// Get touching trigger colliders
			var collidersInMeleeRange = UnitMeleeCollider.Touching;
			// Select colliders belonging to Units
			if ( collidersInMeleeRange.Where( col => (col.Tags.Has( UNIT_TAG ) || col.Tags.Has( Building.BUILDING_TAG )) ).Any())
			{
				// Select only melee colliders
				foreach ( var collision in collidersInMeleeRange.Where( col => (col.Tags.Has( UNIT_TAG ) || col.Tags.Has( Building.BUILDING_TAG )) ))
					//col => col == (col.GameObject.Components.Get<Unit>()).UnitMeleeCollider ))
				{
					//Collider belongs to building
					if ( collision.Tags.Has( Building.BUILDING_TAG ) )
					{
						var buildingCollidedWith = collision.GameObject.Components.GetAll().OfType<Building>().First();
						if ( buildingCollidedWith.team != team )
						{
							if ( Time.Now - lastMeleeTime > MeleeAttackSpeed )
							{
								//Log.Info( this.GameObject.Name + " attacks " + collisions.GameObject.Name + " for " + melee_attack_damage + " damage!" );
								directMeleeAttack( buildingCollidedWith );
							}
						}
					}
					//Collider belongs to unit
					else if ( collision.Tags.Has( UNIT_TAG ) )
					{
							var unitCollidedWith = collision.GameObject.Components.GetAll().OfType<Unit>().First();
							if (collision == unitCollidedWith.UnitMeleeCollider && unitCollidedWith.team != team )
							{
								if ( Time.Now - lastMeleeTime > MeleeAttackSpeed )
								{
									//Log.Info( this.GameObject.Name + " attacks " + collisions.GameObject.Name + " for " + melee_attack_damage + " damage!" );
									directMeleeAttack( unitCollidedWith );
								}
							}
					}
					//var unitCollidedWith = collision.GameObject.Components.GetAll().OfType<Unit>().First();
					// If it is a unit of the opposite team
				}
			}
		}
		// Auto Melee
		if(UnitAutoMeleeCollider != null && currentStance == ((int)Stance.Attack))
		{
			var validUnitFound = false;
			if ( tempTargetObject == null )
			{
				// Get touching trigger colliders
				var collidersInAutoMeleeRange = UnitAutoMeleeCollider.Touching;
				// Select colliders belonging to Units
				if ( collidersInAutoMeleeRange.Where( col => col.Tags.Has( UNIT_TAG ) || col.Tags.Has( Building.BUILDING_TAG ) ).Any() )
				{
					// Select only melee colliders
					foreach ( var collision in collidersInAutoMeleeRange)//.Where( col => (col == (col.GameObject.Components.Get<Unit>()).UnitMeleeCollider) || (col.Tags.Has(Building.BUILDING_TAG) && col == col.GameObject.Components.Get<Building>().SelectionHitbox) ) )
					{
						//Collider belongs to building
						if(collision.Tags.Has( Building.BUILDING_TAG ))
						{
							//TODO
						}
						//Collider belongs to unit
						else if( collision.Tags.Has( UNIT_TAG ) )
						{
							var unitCollidedWith = collision.GameObject.Components.GetAll().OfType<Unit>().First();
							// If it is a unit of the opposite team
							if ( unitCollidedWith.team != team )
							{
								//Log.Info( this.GameObject.Name + " will attack " + collisions.GameObject.Name + "!" );
								tempTargetObject = unitCollidedWith.GameObject;
								validUnitFound = true;
							}
						}
					}
				}
			}
			// Reset automelee if nothing in range
			if(validUnitFound != true) 
			{
				tempTargetObject = null;
			}
		}

		// Handle Animations
		if (PhysicalModelRenderer != null && UnitNavAgent != null) 
		{
			if ( !UnitNavAgent.Velocity.IsNearZeroLength )
			{
				PhysicalModelRenderer.animateMovement(UnitNavAgent.Velocity, UnitNavAgent.WishVelocity);
			}
			else
			{
				PhysicalModelRenderer.stopMovementAnimate();
			}
		}
	}

	public void setStance(int newStance)
	{
		if (!Network.IsOwner) { return; }
		if(newStance >= 0 && newStance < Enum.GetValues<Stance>().Count())
		{
			currentStance = newStance;
		}	
		else
		{
			Log.Error("Invalid Stance!");
		}
	}

	public override void deSelect()
	{
		if (!Network.IsOwner) { return; }
		selected = false;
		PhysicalModelRenderer.setOutlineState( UnitModelUtils.OutlineState.Mine );
		ThisHealthBar.setShowHealthBar(false);
	}

	[Broadcast]
	public override void die()
	{
		//Log.Info( this.GameObject.Name + " dies!" );
		PhysicalModelRenderer.animateDeath();
		UnitNavAgent.Enabled = false;
		UnitMeleeCollider.Enabled = false;
		UnitAutoMeleeCollider.Enabled = false;
		if ( UnitRangedAttackCollider != null )
		{
			UnitRangedAttackCollider.Enabled = false;
		}
		SelectionHitbox.Enabled = false;
		ThisHealthBar.Enabled = false;
		ThisHealthBar.setEnabled( false );
		PhysicalModelRenderer.baseStand.setEnabled( false );
		isAlive = false;
		Enabled = false;

		//This will be fully destroyed later when the corpse dissapears
		PhysicalModelRenderer.addToCorpsePile();
	}

	public void move(Vector3 location, bool isNewMoveCommand)
	{
		if (!Network.IsOwner) { return; }
		if (location != UnitNavAgent.TargetPosition)
		{
			if(isNewMoveCommand || Time.Now - lastMoveOrderTime >= MOVE_ORDER_FREQUENCY )
			{
				lastMoveOrderTime = Time.Now;
				//Log.Info( "Move Command Sent: " + UnitNavAgent.TargetPosition );
				UnitNavAgent.MoveTo( location );
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
		targetUnit.takeDamage( MeleeAttackDamage );
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
		float targetxyMin = float.Min( targetModelSize.x, targetModelSize.y );
		float targetxyMax = float.Max( targetModelSize.x, targetModelSize.y );
		float defaultxyMin = float.Min( defaultModelSize.x, defaultModelSize.y );
		float defaultxyMax = float.Max( defaultModelSize.x, defaultModelSize.y );
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
		SelectionHitbox.Center = new Vector3( 0, 0, defaultModelSize.z / 2 );
		SelectionHitbox.Scale = new Vector3( defaultxyMin * CLICK_HITBOX_RADIUS_MULTIPLIER, defaultxyMin * CLICK_HITBOX_RADIUS_MULTIPLIER, defaultModelSize.z );

		// Auto calculate unit's chase distance
		maxChaseDistanceFromHome = CHASE_DIST_MULTIPLIER * targetxyMax;

		// Auto Calculate other visual element sizes
		PhysicalModelRenderer.setModelSize( defaultModelSize );
		ThisHealthBar.setSize( defaultModelSize );
	}

	public void stanceButtonClicked()
	{
		var newStance = currentStance + 1;
		if (newStance == Enum.GetValues<Stance>().Count())
		{
			newStance = 0;
		}
		setStance( newStance );
		unitStanceButton.setButtonState( newStance );

		/*switch(currentStance)
		{
			case Stance.Attack:

				break;
			
		}*/
		//setIsInAttackMode(false);
		
	}

	public void recycleUnit()
	{
		isAlive = false;
		RTSPlayer.Local.resourcePoints += (int)((float)ResourceCost * ((float)currentHealthPoints / (float)MaxHealth));
		die();

		var selectedObjList = RTSPlayer.Local.UnitControl.SelectedObjects;
		if (selectedObjList.Count == 0)
		{
			RTSPlayer.Local.LocalGame.GameHud.setSelectionVars(false, false, 0);
		}
		else if (selectedObjList.Count == 1)
		{
			RTSPlayer.Local.LocalGame.GameHud.setSelectionVars(true, true, ((Unit)(selectedObjList[0])).currentStance);
		}
		else
		{
			RTSPlayer.Local.LocalGame.GameHud.setSelectionVars(true, false, ((Unit)(selectedObjList[0])).currentStance);
			//focusedUnit = ((Unit)(selectedObjList[0]));
		}

		this.Destroy();
	}

	public void setMoveCommand(Vector3 targetLocation)
	{
		commandGiven = UnitModelUtils.CommandType.Move;
		hasReachedMoveTarget = false;
		isNewCommand = true;
		homeTargetLocation = targetLocation;
	}

	public void setAttackCommand(GameObject newTargetObject)
	{
		commandGiven = UnitModelUtils.CommandType.Attack;
		isNewCommand= true;
		targetObject = newTargetObject;
	}
}
