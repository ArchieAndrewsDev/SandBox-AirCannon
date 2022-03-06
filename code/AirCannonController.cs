using Sandbox;

public partial class AirCannonController : WalkController
{
	[Net, Property] public bool IsRagdolled { get; set; } = false;
	[Net, Property] public Vector3 LaunchedVelocity { get; set; } = Vector3.Zero;

	private bool wasRagdolled = false;

	public AirCannonController()
	{

	}

	public override void Simulate()
	{
		if ( !IsRagdolled )
		{
			base.Simulate();
			return;
		}
		else if ( wasRagdolled == false && IsRagdolled == true)
		{
			ClearGroundEntity();
			Velocity = LaunchedVelocity;
			Log.Warning( LaunchedVelocity.ToString() );
		}
		else if ( GroundEntity != null )
			IsRagdolled = false;

		AirMove();

		Velocity -= new Vector3( 0, 0, Gravity * 0.5f ) * Time.Delta;
		Velocity += new Vector3( 0, 0, BaseVelocity.z ) * Time.Delta;

		BaseVelocity = BaseVelocity.WithZ( 0 );

		wasRagdolled =IsRagdolled;
	}
}
