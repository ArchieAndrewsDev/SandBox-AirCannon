﻿using Sandbox;
using System;

partial class AirCannonPlayer : Player
{
	private float forceThreshold = 1;
	private bool isRagdolled = false;

	private float ragdolledTime;
	private float minDollTime => 2f;

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

		Controller = new AirCannonController();
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

	public void HitWithForce(Vector3 direction, float force )
	{
		isRagdolled = true;
		ragdolledTime = Time.Now;

		if ( Controller is AirCannonController airCannonController )
		{
			Log.Warning("Hit");
			airCannonController.IsRagdolled = true;
			airCannonController.LaunchedVelocity = (direction * force);

			CameraMode = new RagdollCamera();
		}

		BecomeRagdollOnClient( this );
	}

	private void GetUpFromRagdoll()
	{
		isRagdolled = false;

		if ( Controller is AirCannonController airCannonController )
		{
			airCannonController.IsRagdolled = false;

			CameraMode = new FirstPersonCamera();
		}

		if(Corpse != null)
			Corpse.Delete();
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

		DebugTools();

		if ( LifeState != LifeState.Alive)
			return;

		var controller = GetActiveController();
		if ( controller != null )
			EnableSolidCollisions = !controller.HasTag( "noclip" );

		TickPlayerUse();
		SimulateActiveChild( cl, ActiveChild );

		if ( isRagdolled )
		{
			//Detect getting out of ragdoll
			if ( controller.GroundEntity != null && (Time.Now - ragdolledTime) >= minDollTime )
				GetUpFromRagdoll();

			if ( Corpse != null )
				Corpse.PhysicsBody.Velocity = Position - Corpse.Position;
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
			HitWithForce( EyeRotation.Forward, 1500 );
		}

		if ( IsServer && Input.Pressed( InputButton.Slot3 ) )
		{
			GetUpFromRagdoll();
		}
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
