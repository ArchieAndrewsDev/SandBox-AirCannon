using Sandbox;

partial class AirCannonPlayer
{
	private void BecomeRagdollOnServer( Vector3 velocity, Vector3 direction, Vector3 force)
	{
		if ( ragdollEntity != null)
			return;

		isRagdolled = true;
		EnableAllCollisions = false;
		EnableShadowCasting = false;
		EnableDrawing = false;
		EnableViewmodelRendering = false;

		ClientRagdoll();

		ragdollEntity = new ModelEntity();
		ragdollEntity.Position = Position;
		ragdollEntity.Rotation = Rotation;
		ragdollEntity.Scale = Scale;
		ragdollEntity.MoveType = MoveType.Physics;
		ragdollEntity.UsePhysicsCollision = true;
		ragdollEntity.EnableAllCollisions = true;
		ragdollEntity.CollisionGroup = CollisionGroup.Player;
		ragdollEntity.SetModel( GetModelName() );
		ragdollEntity.CopyBonesFrom( this );
		ragdollEntity.CopyBodyGroups( this );
		ragdollEntity.CopyMaterialGroup( this );
		ragdollEntity.TakeDecalsFrom( this );
		ragdollEntity.EnableHitboxes = true;
		ragdollEntity.EnableAllCollisions = true;
		ragdollEntity.SurroundingBoundsMode = SurroundingBoundsType.Physics;
		ragdollEntity.RenderColor = RenderColor;
		ragdollEntity.PhysicsGroup.Velocity = velocity + (direction * force);

		ragdollEntity.SetInteractsAs( CollisionLayer.Player );
		ragdollEntity.SetInteractsWith( CollisionLayer.WORLD_GEOMETRY );
		ragdollEntity.SetInteractsExclude( CollisionLayer.Player | CollisionLayer.Debris );

		RagdollController ragdollController = new RagdollController();
		ragdollController.ragdoll = ragdollEntity.PhysicsBody;
		Controller = ragdollController;

		CameraMode = new OrbitEntityCamera();
		CameraMode.Viewer = this;

		Animator = null;

		foreach ( var child in Children )
		{
			if ( !child.Tags.Has( "clothes" ) ) continue;
			if ( child is not ModelEntity e ) continue;

			var model = e.GetModelName();

			var clothing = new ModelEntity();
			clothing.SetModel( model );
			clothing.SetParent( ragdollEntity, true );
			clothing.RenderColor = e.RenderColor;
			clothing.CopyBodyGroups( e );
			clothing.CopyMaterialGroup( e );
		}
	}

	private void GetUpFromRagDoll()
	{
		isRagdolled = false;
		EnableAllCollisions = true;
		EnableDrawing = true;
		Controller = new WalkController();
		CameraMode = new FirstPersonCamera();
		Animator = new StandardPlayerAnimator();

		ragdollEntity.Delete();
		ragdollEntity = null;

		ClientGetUpFromRagdoll();
	}

	[ClientRpc]
	private void ClientRagdoll()
	{
		CameraMode.Viewer = this;
	}

	[ClientRpc]
	private void ClientGetUpFromRagdoll()
	{
		CameraMode.Viewer = null;
	}
}
