using Sandbox;
using System;

partial class AirCannonPlayer : Player
{
	private bool isRagdolled = false; 
	private float ragdolledTime; //Time since player was ragdolled
	private float minDollTime = 2f; //The min time that the player can be ragdolled 
	private float ragdollCorrectionSpeed = 40; //The speed that the ragdoll moves towards the players position

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
		//Load character model
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

		//Dress Terry in his drip
		Clothing.DressEntity( this );

		CameraMode = new FirstPersonCamera();

		Inventory.Add( new Cannon(), true);

		base.Respawn();
	}

	public void HitWithForce(Vector3 force )
	{
		isRagdolled = true;
		//Store time of ragdoll to ensure players don't get up to fast
		ragdolledTime = Time.Now;

		//Disable character model so only ragdoll is visible
		EnableDrawing = false;

		//Stop drawing children (Weapons)
		foreach ( var child in Children )
		{
			child.EnableDrawing = false;
		}

		//Grab the AirCannonController from Controller and tell it we have ragdolled. 
		if ( Controller is AirCannonController airCannonController )
		{
			airCannonController.IsRagdolled = true;
			airCannonController.LaunchedVelocity = (force);

			CameraMode = new RagdollCamera();
		}

		//Create a local ragdoll and attatch it to the player
		BecomeRagdollOnClient( force );
	}

	private void GetUpFromRagdoll()
	{
		isRagdolled = false;
		EnableDrawing = true;

		foreach ( var child in Children )
		{
			child.EnableDrawing = true;
		}

		if ( Controller is AirCannonController airCannonController )
		{
			airCannonController.IsRagdolled = false;

			CameraMode = new FirstPersonCamera();
		}

		CleaRagdoll();
	}

	public override PawnController GetActiveController()
	{
		if ( DevController != null ) return DevController;

		return base.GetActiveController();
	}

	public override void Simulate( Client cl )
	{
		base.Simulate( cl );

		//Some hotkeys to test functions
		DebugTools();

		if ( Input.ActiveChild != null )
		{
			ActiveChild = Input.ActiveChild;
		}

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
		}

		//Get each client to move the ragdoll to the players position
		if ( IsClient )
		{
			if ( Corpse != null )
			{
				Vector3 direction = Position - Corpse.PhysicsBody.Position + Vector3.Up;
				Corpse.PhysicsBody.Velocity = Velocity + direction * ragdollCorrectionSpeed;
			}
		}
	}

	public override void FrameSimulate( Client cl )
	{
		base.FrameSimulate( cl );
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
			HitWithForce(EyeRotation.Forward * 1500 + Vector3.Up * 100 );
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
