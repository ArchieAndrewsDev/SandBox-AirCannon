using Sandbox;

partial class AirCannonPlayer
{
	[ClientRpc]
	private void BecomeRagdollOnClient( Vector3 force )
	{
		ModelEntity ragdollEntity = new ModelEntity();
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

		ragdollEntity.SetInteractsAs( CollisionLayer.Player );
		ragdollEntity.SetInteractsWith( CollisionLayer.WORLD_GEOMETRY );
		ragdollEntity.SetInteractsExclude( CollisionLayer.Player | CollisionLayer.Debris );

		Corpse = ragdollEntity;

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

		if ( ragdollEntity.PhysicsGroup != null )
		{
			var angularDir = (Rotation.FromYaw( 90 ) * force.WithZ( 0 ).Normal).Normal;
			ragdollEntity.PhysicsGroup.AddAngularVelocity( angularDir * (force.Length * 0.02f) );
		}
	}

	[ClientRpc]
	private void CleaRagdoll()
	{
		Corpse.Delete();
		Corpse = null;
	}
}
