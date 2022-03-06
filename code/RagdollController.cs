using Sandbox;

public partial class RagdollController : BasePlayerController
{
	[Net] private float BodyGirth { get; set; } = 32.0f;
	[Net] private float BodyHeight { get; set; } = 72.0f;
	[Net] private Vector3 startVelcoity { get; set; }

	public RagdollController()
	{

	}

	public RagdollController( Vector3 newVelocity )
	{
		Log.Warning(newVelocity.ToString());
		startVelcoity = newVelocity;
	}

	public override void Simulate()
	{
		Log.Warning( startVelcoity.ToString() );

		MoveHelper moveHelper = new MoveHelper(Position, Velocity);
		moveHelper.Trace = moveHelper.Trace.Size( GetHull() ).Ignore( Pawn );

		moveHelper.TryMove( Time.Delta );

		Position = moveHelper.Position;	
		Velocity = moveHelper.Velocity;

		WishVelocity = Velocity;
		GroundEntity = null;
		BaseVelocity = Vector3.Zero;
	}

	public override BBox GetHull()
	{
		var girth = BodyGirth * 0.5f;
		var mins = new Vector3( -girth, -girth, 0 );
		var maxs = new Vector3( +girth, +girth, BodyHeight );

		return new BBox( mins, maxs );
	}
}
