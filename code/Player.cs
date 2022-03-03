﻿using Sandbox;
using System;

partial class AirCannonPlayer : Player
{
	private TimeSince timeSinceDropped;
	private TimeSince timeSinceJumpReleased;
	private ModelEntity ragdollEntity = null;

	[Net]
	private bool isRagdolled { get; set; } = false;

	private float forceThreshold = 1;

	/// <summary>
	/// The clothing container is what dresses the citizen
	/// </summary>
	public Clothing.Container Clothing = new();

	/// <summary>
	/// Default init
	/// </summary>
	public AirCannonPlayer()
	{
		Inventory = new Inventory( this );
	}

	/// <summary>
	/// Initialize using this client
	/// </summary>
	public AirCannonPlayer( Client cl ) : this()
	{
		// Load clothing from client data
		Clothing.LoadFromClient( cl );
	}

	public override void Respawn()
	{
		SetModel( "models/citizen/citizen.vmdl" );

		Controller = new WalkController();
		Animator = new StandardPlayerAnimator();

		if ( DevController is NoclipController )
		{
			DevController = null;
		}

		EnableAllCollisions = true;
		EnableDrawing = true;
		EnableHideInFirstPerson = true;
		EnableShadowInFirstPerson = true;

		Clothing.DressEntity( this );

		CameraMode = new FirstPersonCamera();

		Inventory.Add( new Cannon(), true);

		base.Respawn();
	}

	public void HitWithForce(Vector3 position, Vector3 force )
	{
		float magnitude = Magnitude(force);
		if ( forceThreshold <= magnitude)
		{
			BecomeRagdollOnServer( Vector3.Zero, DamageFlags.Blast, position, force, 0 );
		}
	}

	private float Magnitude(Vector3 vector)
	{
		float m = vector.x * vector.x + vector.y * vector.y + vector.z * vector.z;
		m = MathF.Sqrt( m );
		return m;
	}

	public override PawnController GetActiveController()
	{
		if ( DevController != null ) return DevController;

		return base.GetActiveController();
	}

	public override void Simulate( Client cl )
	{
		base.Simulate( cl );

		if ( Input.ActiveChild != null )
		{
			ActiveChild = Input.ActiveChild;
		}

		if(ragdollEntity != null && isRagdolled)
		{
			Position = ragdollEntity.Position;
			var movement = new Vector3( Input.Forward, Input.Left, 0 ).Normal;
			ragdollEntity.Velocity = movement * 1000;		
		}

		DebugTools();

		if ( LifeState != LifeState.Alive || isRagdolled)
			return;

		Log.Warning( isRagdolled );

		var controller = GetActiveController();
		if ( controller != null )
			EnableSolidCollisions = !controller.HasTag( "noclip" );

		TickPlayerUse();
		SimulateActiveChild( cl, ActiveChild );

		if ( Input.Released( InputButton.Jump ) )
		{
			if ( timeSinceJumpReleased < 0.3f )
			{
				Game.Current?.DoPlayerNoclip( cl );
			}

			timeSinceJumpReleased = 0;
		}

		if ( Input.Left != 0 || Input.Forward != 0 )
		{
			timeSinceJumpReleased = 1;
		}
	}

	private void DebugTools()
	{
		if ( IsServer && Input.Pressed( InputButton.Slot1 ) )
		{
			var testProp = new ModelEntity();
			testProp.SetModel( "models/sbox_props/bin/street_bin.vmdl_c" );

			TraceResult tr = Trace.Ray( EyePosition, EyePosition + EyeRotation.Forward )
				.Ignore( this )
				.Run();

			testProp.Position = (tr.Hit) ? tr.EndPosition : EyePosition + EyeRotation.Forward * 100;
			testProp.Rotation = Rotation.LookAt( tr.Normal );
			testProp.SetupPhysicsFromModel( PhysicsMotionType.Dynamic, false );
		}

		if ( IsServer && Input.Pressed( InputButton.Slot2 ) )
		{
			if ( isRagdolled )
			{
				GetUpFromRagDoll();
			}
			else
			{
				HitWithForce( Position, Vector3.Up * 600 );
			}
		}
	}

	public override void StartTouch( Entity other )
	{
		if ( timeSinceDropped < 1 ) return;

		base.StartTouch( other );
	}

	[ServerCmd( "inventory_current" )]
	public static void SetInventoryCurrent( string entName )
	{
		var target = ConsoleSystem.Caller.Pawn as Player;
		if ( target == null ) return;

		var inventory = target.Inventory;
		if ( inventory == null )
			return;

		for ( int i = 0; i < inventory.Count(); ++i )
		{
			var slot = inventory.GetSlot( i );
			if ( !slot.IsValid() )
				continue;

			if ( !slot.ClassInfo.IsNamed( entName ) )
				continue;

			inventory.SetActiveSlot( i, false );

			break;
		}
	}
}
