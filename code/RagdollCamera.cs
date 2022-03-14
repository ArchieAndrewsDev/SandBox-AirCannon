using Sandbox;

public class RagdollCamera : CameraMode
{
	public virtual Vector3 FocusPoint { get; set; }

	public override void Activated()
	{
		base.Activated();

		FocusPoint = CurrentView.Position - GetViewOffset();
		FieldOfView = CurrentView.FieldOfView;
	}

	public override void Update()
	{
		var player = Local.Client;
		if ( player == null ) return;

		// lerp the focus point
		//FocusPoint = Vector3.Lerp( FocusPoint, GetSpectatePoint(), Time.Delta * 5.0f );
		FocusPoint = GetSpectatePoint();

		Position = FocusPoint + GetViewOffset();
		Rotation = Input.Rotation;
		FieldOfView = FieldOfView.LerpTo( 50, Time.Delta * 3.0f );

		Viewer = null;
	}

	public virtual Vector3 GetSpectatePoint()
	{
		return Local.Pawn.Position;
	}

	public virtual Vector3 GetViewOffset()
	{
		var player = Local.Client;
		if ( player == null ) return Vector3.Zero;

		return Input.Rotation.Forward * (-130 * 1) + Vector3.Up * (20 * 1);
	}
}
