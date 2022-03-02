using Sandbox;
using System;
using System.Linq;

partial class Inventory : BaseInventory
{
	public Inventory( Player player ) : base( player )
	{

	}

	public override bool Add( Entity entity, bool makeActive = false )
	{
		var player = Owner as Player;
		var cannon = entity as Cannon;

		return base.Add( entity, makeActive );
	}

	public bool IsCarryingType( Type t )
	{
		return List.Any( x => x.GetType() == t );
	}
}
