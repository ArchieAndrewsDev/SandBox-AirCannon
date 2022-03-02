
using Sandbox;
using Sandbox.UI.Construct;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

//
// You don't need to put things in a namespace, but it doesn't hurt.
//
namespace Sandbox
{
	/// <summary>
	/// This is your game class. This is an entity that is created serverside when
	/// the game starts, and is replicated to the client. 
	/// 
	/// You can use this to create things like HUDs and declare which player class
	/// to use for spawned players.
	/// </summary>
	public partial class MyGame : Sandbox.Game
	{
		public MyGame()
		{
			if ( IsServer )
			{
				// Create the HUD
				var hud = new AirCannonHud();
				hud.Parent = this; // Do not delete me on map cleanup
			}
		}

		/// <summary>
		/// A client has joined the server. Make them a pawn to play with
		/// </summary>
		public override void ClientJoined( Client client )
		{
			base.ClientJoined( client );

			var player = new AirCannonPlayer( client );
			player.Respawn();

			client.Pawn = player;

			var testPlayer = new AirCannonPlayer( client );
			testPlayer.Respawn();
		}
	}

}
