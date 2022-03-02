using Sandbox;
using Sandbox.UI;

[Library]
public partial class AirCannonHud : HudEntity<RootPanel>
{
	public AirCannonHud()
	{
		if ( !IsClient )
			return;

		RootPanel.StyleSheet.Load( "/ui/AirCannonHud.scss" );
		
		RootPanel.AddChild<CannonUI>();
	}
}
