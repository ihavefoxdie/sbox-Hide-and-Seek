using Sandbox;
using Sandbox.ModelEditor.Nodes;
using System;
using System.Linq;

namespace HideAndSeek;

public class PawnUIManager : Component
{
	/// <summary>
	/// UI prefab to use for pawns.
	/// </summary>
	[Property] public GameObject UIPrefab {  get; set; }
	/// <summary>
	/// Sync component that serves as a bridge between services.
	/// </summary>
	private SyncComponent Sync {  get; set; }
	/// <summary>
	/// This boolean signifies component readiness.
	/// </summary>
	public bool GreenLight = false;
	
	protected override void OnStart()
	{
		base.OnStart();

		Sync = Scene.GetAllComponents<SyncComponent>().Last();

		if(Sync != null && Sync.CurrentGame != null)
		{
			Sync.CurrentGame.OnPawnSpawn += CreateUIObject;
			Sync.CurrentGame.OnPawnRemoval += RemoveUIObject;
			Sync.CurrentGame.OnPawnCleanUp += RemoveAllUIObjects;
		}
		GreenLight = true;
	}

	protected override void OnUpdate()
	{

	}

	#region Methods

	/// <summary>
	/// Creates UI GameObject for the specified pawn.
	/// </summary>
	/// <param name="pawnId">GameObject ID of the pawn.</param>
	/// <param name="connection">Player connection.</param>
	private void CreateUIObject(Guid pawnId, Connection connection)
	{
		if ( UIPrefab == null )
			return;
		GameObject ui = UIPrefab.Clone( GameObject, GameObject.Transform.Position, GameObject.Transform.Rotation, GameObject.Transform.Scale );
		ui.NetworkSpawn( connection );
		ui.NetworkMode = NetworkMode.Snapshot;
	}

	[Broadcast]
	private void RemoveAllUIObjects()
	{
		for ( int i = 0; i < GameObject.Children.Count; i++ )
		{
			GameObject.Children[i].Destroy();
		}
	}

	[Broadcast]
	private void RemoveUIObject( Guid pawnId )
	{
		for ( int i = 0; i < GameObject.Children.Count; i++ )
		{
			PawnSyncComponent pawnComp = GameObject.Children[i].Components.Get<PawnSyncComponent>( true );
			if ( pawnComp != null && pawnComp.Pawn.GameObject.Id == pawnId )
			{
				GameObject.Children[i].Destroy();
				return;
			}
		}
	}

	#endregion
}
