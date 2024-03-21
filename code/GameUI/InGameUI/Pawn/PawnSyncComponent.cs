using Sandbox;
using System.Linq;

namespace HideAndSeek;

public class PawnSyncComponent : Component
{
	[Property] public PawnComponent Pawn { get; set; }
	[Property] public SyncComponent SyncComp { get; set; }

	protected override void OnStart()
	{
		
		var syncComp = Scene.GetAllComponents<SyncComponent>().Last();
		var everyPawnComp = Scene.Components.GetAll<PawnComponent>(FindMode.EverythingInDescendants);
		if ( syncComp != null )
		{
			SyncComp = syncComp;
		}
		if ( everyPawnComp != null )
		{
			var thePawnComp = everyPawnComp.Where( x => x.Network.OwnerConnection == this.Network.OwnerConnection ).Last();
			if ( thePawnComp != null )
			{
				Pawn = thePawnComp;
			}
		}
	}

	protected override void OnUpdate()
	{
	}

	protected override void OnFixedUpdate()
	{
		base.OnFixedUpdate();
	}
}
