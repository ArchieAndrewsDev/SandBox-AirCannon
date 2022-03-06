using Sandbox;

public class RagdollController : BasePlayerController
{
	public PhysicsBody ragdoll;

	private float trackSpeed => 5;

	public override void Simulate()
	{
		base.Simulate();

		if(ragdoll != null )
		{
			Vector3 velocity = ragdoll.Position - Position;
			velocity *= trackSpeed;
			MoveHelper mover = new MoveHelper(Position, velocity);
			mover.TryMove( Time.Delta );
			Position = mover.Position;

			//Position = Vector3.Lerp( Position, mover.Position, RealTime.Delta * 20.0f );
		}
	}
}
