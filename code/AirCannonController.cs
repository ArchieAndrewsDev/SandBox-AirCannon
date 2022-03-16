using Sandbox;
using System;

public partial class AirCannonController : WalkController
{
	[Net]public bool IsRagdolled { get; set; } = false;
	[Net]public Vector3 LaunchedVelocity { get; set; } = Vector3.Zero;

	private bool wasRagdolled = false;

	public AirCannonController()
	{

	}

	private float Magnitude( Vector3 vector )
	{
		float m = vector.x * vector.x + vector.y * vector.y + vector.z * vector.z;
		m = MathF.Sqrt( m );
		return m;
	}


	public override void Simulate()
	{
		//Log.Warning( string.Format("{0} - {1}", Position, IsRagdolled) ) ;

		if ( !IsRagdolled )
		{
			base.Simulate();
		}
		else if ( wasRagdolled == false && IsRagdolled == true)
		{
			Log.Warning("Start Ragdoll");
			ClearGroundEntity();
			Velocity = LaunchedVelocity;
		}
		else
		{
			AirMove();

			Velocity -= new Vector3( 0, 0, Gravity * 0.5f ) * Time.Delta;
			Velocity += new Vector3( 0, 0, BaseVelocity.z ) * Time.Delta;

			BaseVelocity = BaseVelocity.WithZ( 0 );

			var point = Position - Vector3.Up * 2;
			var vBumpOrigin = Position;

			var pm = TraceBBox( vBumpOrigin, point, 4.0f );

			if ( pm.Entity == null || Vector3.GetAngle( Vector3.Up, pm.Normal ) > GroundAngle )
			{
				ClearGroundEntity();

				if ( Velocity.z > 0 )
					SurfaceFriction = 0.25f;
			}
			else
			{
				UpdateGroundEntity( pm );
			}
		}

		wasRagdolled = IsRagdolled;
	}
}
