using Sandbox;

partial class AirCannonPlayer
{
	private void BecomeRagdollOnServer( Vector3 velocity, DamageFlags damageFlags, Vector3 forcePos, Vector3 force, int bone )
	{
		if ( ragdollEntity != null )
			return;

		isRagdolled = true;
		EnableAllCollisions = false;
		EnableDrawing = false;
		Controller = new FlyingController();
		CameraMode = new FollowRagdollCamera();

		ragdollEntity = new ModelEntity();
		ragdollEntity.Position = Position;
		ragdollEntity.Rotation = Rotation;
		ragdollEntity.Scale = Scale;
		ragdollEntity.MoveType = MoveType.Physics;
		ragdollEntity.UsePhysicsCollision = true;
		ragdollEntity.EnableAllCollisions = true;
		ragdollEntity.CollisionGroup = CollisionGroup.Debris;
		ragdollEntity.SetModel( GetModelName() );
		ragdollEntity.CopyBonesFrom( this );
		ragdollEntity.CopyBodyGroups( this );
		ragdollEntity.CopyMaterialGroup( this );
		ragdollEntity.TakeDecalsFrom( this );
		ragdollEntity.EnableHitboxes = true;
		ragdollEntity.EnableAllCollisions = true;
		ragdollEntity.SurroundingBoundsMode = SurroundingBoundsType.Physics;
		ragdollEntity.RenderColor = RenderColor;
		ragdollEntity.PhysicsGroup.Velocity = velocity;

		ragdollEntity.SetInteractsAs( CollisionLayer.Debris );
		ragdollEntity.SetInteractsWith( CollisionLayer.WORLD_GEOMETRY );
		ragdollEntity.SetInteractsExclude( CollisionLayer.Player | CollisionLayer.Debris );

		ragdollEntity.PhysicsBody.ApplyImpulse( force );

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

		/*
		if ( damageFlags.HasFlag( DamageFlags.Bullet ) ||
			 damageFlags.HasFlag( DamageFlags.PhysicsImpact ) )
		{
			PhysicsBody body = bone > 0 ? ent.GetBonePhysicsBody( bone ) : null;

			if ( body != null )
			{
				body.ApplyImpulseAt( forcePos, force * body.Mass );
			}
			else
			{
				ent.PhysicsGroup.ApplyImpulse( force );
			}
		}
		*/
	}

	private void GetUpFromRagDoll()
	{
		isRagdolled = false;
		EnableAllCollisions = true;
		EnableDrawing = true;
		Controller = new WalkController();
		CameraMode = new FirstPersonCamera();

		ragdollEntity.Delete();
		ragdollEntity = null;
	}
}
