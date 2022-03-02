using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

public class CannonUI : Panel
{
	public Label chargeValue;
	public Slider chargeBar;

	public CannonUI()
	{
		chargeValue = Add.Label("0", "ChargeLevel" );
		//chargeBar = Add.Slider( 0, 1, 0 );
	}

	public override void Tick()
	{
		var currentCannon = GetCurrentCannon();

		chargeValue.Text = $"{(currentCannon.cannonCharge * 100) / 100}";
		//chargeBar.Value = currentCannon.cannonCharge / currentCannon.maxCannonCharge;
	}

	Cannon GetCurrentCannon()
	{
		var player = Local.Pawn as Player;
		if ( player == null ) return null;

		var inventory = player.Inventory;
		if ( inventory == null ) return null;

		if ( inventory.Active is not Cannon cannon ) return null;

		return cannon;
	}
}
