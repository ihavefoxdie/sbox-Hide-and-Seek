using Sandbox;
using System.Collections.Generic;
using System.Linq;

namespace HideAndSeek;

public class PawnSyncComponent : Component
{
	[Property] public PawnComponent Pawn { get; set; }
	[Property] public SyncComponent SyncComp { get; set; }

	protected override void OnStart()
	{
		GameObject parent = GameObject.Parent;
		Log.Info( parent );
		var syncComp = Scene.GetAllComponents<SyncComponent>().Last();
		Log.Info( syncComp );

		if (syncComp != null )
		{
			SyncComp = syncComp;
		}
		if ( parent != null )
		{
			Pawn = parent.Components.Get<PawnComponent>(true);
		}
	}

	protected override void OnUpdate()
	{
		if ( IsProxy ) return;
		if ( Pawn == null ) return;
		if ( SyncComp == null ) return;
	}
}
