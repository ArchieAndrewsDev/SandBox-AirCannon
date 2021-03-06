using Sandbox;

public partial class Cannon : BaseWeapon
{
	public override string ViewModelPath => "weapons/rust_pistol/v_rust_pistol.vmdl";

	[Net, Predicted]
	public float cannonCharge { get; set; }
	public virtual float maxCannonCharge => 1;
	public virtual float chargeSpeed => 10f;
	public virtual float range => 100000;
	public virtual float force => 2000;
	public virtual float upForce => 400;

	public override void Spawn()
	{
		base.Spawn();

		SetModel( "weapons/rust_pistol/rust_pistol.vmdl" );
	}

	public override void CreateViewModel()
	{
		Host.AssertClient();

		if ( string.IsNullOrEmpty( ViewModelPath ) )
			return;

		ViewModelEntity = new ViewModel
		{
			Position = Position,
			Owner = Owner,
			EnableViewmodelRendering = true
		};

		ViewModelEntity.SetModel( ViewModelPath );
	}

	public override void AttackPrimary()
	{
		if(cannonCharge >= maxCannonCharge )
		{
			ShootEffects();
			PlaySound( "physics.wood.impact.soft" );
			cannonCharge = 0;
			Shoot();
		}
		else
		{
			PlaySound( "ui.button.deny" );
		}
	}

	public override void AttackSecondary()
	{
		base.AttackSecondary();

		if(cannonCharge < maxCannonCharge )
		{
			cannonCharge += Time.Delta * chargeSpeed;
			cannonCharge = MathX.Clamp(cannonCharge, 0, maxCannonCharge);
		}
	}

	public override void Simulate( Client player )
	{
		base.Simulate( player );
	}

	public virtual void Shoot()
	{
		Vector3 eyePosition = Client.Pawn.EyePosition;
		Vector3 eyeForward = Client.Pawn.EyeRotation.Forward;

		//Trace everything in the path of the air blast
		TraceResult[] tr = Trace.Ray( eyePosition, eyePosition + eyeForward * range )
			.Radius( 10 )
			.Ignore( this )
			.RunAll();

		foreach ( TraceResult r in tr )
		{
			if ( r.Hit )
			{
				//The force which we apply to the target
				Vector3 impulse = eyeForward * r.Body.Mass * force;

				//Add some up force to take the player off the ground
				impulse += (Vector3.Up * r.Body.Mass * upForce);

				//If we find a standard physics body apply force to it
				if ( r.Body != null )
				{
					r.Body.ApplyImpulse( impulse );
					DebugOverlay.TraceResult( r );
				}

				if ( !IsServer ) continue;
				if ( !r.Entity.IsValid() ) continue;

				using ( Prediction.Off() )
				{
					//Hit AirCannonPlayers with a force to start the ragdoll
					if ( r.Entity is AirCannonPlayer target && target != null )
					{
						target.HitWithForce( impulse );
					}
				}
			}
		}
	}

	[ClientRpc]
	protected virtual void ShootEffects()
	{
		Host.AssertClient();
		Particles.Create( "particles/pistol_muzzleflash.vpcf", EffectEntity, "muzzle" );

		if ( IsLocalPawn )
		{
			_ = new Sandbox.ScreenShake.Perlin();
		}

		ViewModelEntity?.SetAnimParameter( "fire", true );
		CrosshairPanel?.CreateEvent( "fire" );
	}
}
