using Sandbox;
using Sandbox.GameLogic.Modules;
using System;
using System.Collections.Generic;
using System.Linq;


namespace HideAndSeek;


public class GameComponent : Component, Component.INetworkListener
{
	#region Properties
	[Property] public GameObject PawnUIPrefab { get; set; }
	[Property] public MapInstance CurrentMap {  get; private set; }
	/// <summary>
	/// Current round class object.
	/// </summary>
	public Round CurrentRound { get; set; }
	/// <summary>
	/// Hiders team class object.
	/// </summary>
	public Team Hiders { get; set; }
	/// <summary>
	/// Seekers team class object.
	/// </summary>
	public Team Seekers { get; set; }
	/// <summary>
	/// Guid list for every player pawn in the scene.
	/// </summary>
	[Sync] public List<Guid> PlayerPawns { get; set; }
	[Sync] public bool MapLoaded { get; set; } = false;
	/// <summary>
	/// Tweaked network helper.
	/// </summary>
	[Property] public NetworkComponent NetworkComponent { get; set; }
	/// <summary>
	/// A simple boolean that tells whether the game has begun or not.
	/// </summary>
	[Sync] public bool GameBegan { get; private set; } = false;
	/// <summary>
	/// Round length limit in seconds.
	/// </summary>
	[Property] public int RoundLength { get; set; } = 300;
	/// <summary>
	/// Time before the next round starts.
	/// </summary>
	[Property] public int RoundCooldown { get; set; } = 10;
	/// <summary>
	/// Time before seekers become active.
	/// </summary>
	[Property] public int PreparationTime { get; set; } = 30;
	#endregion


	#region Actions and Events
	public event Action<Team> OnTeamLost;
	public event Action OnRoundStart;
	public event Action OnCaught;
	#endregion


	protected override void OnAwake()
	{
		PlayerPawns = new();
	}
	protected override void OnStart()
	{
	//CurrentMap.IsLoaded
	}

	public void OnActive( Connection conn )
	{
		if ( CurrentRound != null )
		{
			if ( GameBegan )
				return;
		}

		if ( Networking.Connections.Count > 1 )
		{
			GameBegan = true;
			InitGame();
		}
	}


	public void OnConnected( Connection conn )
	{
	}

	public void OnDisconnected( Connection conn )
	{
		for ( int i = 0; i < PlayerPawns.Count; i++ )
		{
			var p = Scene.Directory.FindByGuid( PlayerPawns[i] );
			if ( p != null )
			{
				if ( p.Network.OwnerConnection == conn )
				{
					PlayerPawns.Remove( PlayerPawns[i] );
					break;
				}
			}
		}

		Seekers.TeamPlayers.Remove( conn.Id );
		Hiders.TeamPlayers.Remove( conn.Id );
	}

	protected override void OnFixedUpdate()
	{
		if ( !GameBegan ) { return; }

		if ( CurrentRound is null || Seekers is null || Hiders is null ) return;

		if ( CurrentRound.IsStarted )
		{
			Team lost = LostTeam();
			if ( lost != null )
			{
				Log.Info( "Someone has lost!" );
				OnTeamLost?.Invoke( lost );
				CurrentRound.EndTheRound();
			}
			else if ( CurrentRound.CheckRoundTime() )
			{
				OnTeamLost?.Invoke( Seekers );
				CurrentRound.EndTheRound();
			}
		}

	}

	//TODO: add more documentation.
	#region Methods
	/// <summary>
	/// Assigns caught hider to the seekers team.
	/// </summary>
	/// <param name="catcher">Guid of the seeker's object.</param>
	/// <param name="caught">Guid of the hider's object.</param>
	public void Caught( Guid catcher, Guid caught )
	{
		GameObject catcherObject = Scene.Directory.FindByGuid( catcher );
		GameObject caughtObject = Scene.Directory.FindByGuid( caught );
		if ( catcherObject != null && caughtObject != null )
		{
			if ( catcherObject.Tags.Has( "seekers" ) && caughtObject.Tags.Has( "hiders" ) )
			{
				if ( ChangeTeam( caught, "seekers" ) )
				{
					OnCaught?.Invoke();

				}
			}
		}
	}

	[Broadcast]
	private void SwapTags( Guid objectID, string remove, string add )
	{
		GameObject playerObject = Scene.Directory.FindByGuid( objectID );
		playerObject.Tags.Remove( remove );
		playerObject.Tags.Add( add );
	}

	[Broadcast]
	private void AddSeekerComponent( Guid objectID )
	{
		var pawn = Scene.Directory.FindByGuid( objectID );
		pawn.Components.Create<TeamEquipmentComponent>( true );
	}

	[Broadcast]
	private void RemoveSeekerComponent( Guid objectID )
	{
		var pawn = Scene.Directory.FindByGuid( objectID );
		if(pawn == null ) return;

		var comp = pawn.Components.Get<TeamEquipmentComponent>( true );
		if ( comp == null ) return;

		comp.Destroy();
	}

	/// <summary>
	/// Swaps the team for a player if one is found.
	/// </summary>
	/// <param name="objectID">Player pawn Guid.</param>
	/// <param name="team">Team name (either "seekers" or "hiders").</param>
	/// <returns>If successful - true, otherwise - false.</returns>
	private bool ChangeTeam( Guid objectID, string team )
	{
		GameObject playerObject = Scene.Directory.FindByGuid( objectID );
		if ( playerObject == null ) return false;

		Connection playerConnection = playerObject.Network.OwnerConnection;
		if ( playerConnection == null ) return false;

		switch ( team )
		{
			case "seekers":
				SwapTags( objectID, "hiders", "seekers" );
				AddSeekerComponent( objectID );
				Hiders.TeamPlayers.Remove( playerConnection.Id );
				Seekers.TeamPlayers.Add( playerConnection.Id );
				break;

			case "hiders":
				SwapTags( objectID, "seekers", "hiders" );
				Hiders.TeamPlayers.Add( playerConnection.Id );
				Seekers.TeamPlayers.Remove( playerConnection.Id );
				break;

			default: break;
		}
		return true;
	}

	/// <summary>
	/// Logs team that has lost.
	/// </summary>
	/// <param name="team">Lost team.</param>
	private void LogTeamLost( Team team )
	{
		if ( team != null )
			Log.Info( team.Name + " team has lost!" );
	}

	/// <summary>
	/// Searches for seekers pawns and toggles them or on off (used to give hiders time to run and hide).
	/// </summary>
	/// <param name="toggle">true - enable; false - disable.</param>
	private void ToggleSeekers( bool toggle )
	{
		var pawns = Scene.GetAllObjects( false ).Where( x => x.Tags.Has( "seekers" ) );
		foreach ( var pawn in pawns )
		{
			TogglePawn( pawn.Id, toggle );
		}
	}

	/// <summary>
	/// Toggles Enabled on player pawn.
	/// </summary>
	/// <param name="guid"></param>
	/// <param name="toggle"></param>
	[Broadcast]
	private void TogglePawn( Guid guid, bool toggle )
	{
		var pawn = Scene.Directory.FindByGuid( guid );
		if ( pawn != null )
		{
			pawn.Enabled = toggle;
			if ( toggle == true && pawn.Tags.Has( "seekers" ) )
			{
				pawn.Components.Create<TeamEquipmentComponent>( true );
			}
		}
	}

	/// <summary>
	/// Toggles spectator cam on or off.
	/// </summary>
	/// <param name="toggle"></param>
	[Broadcast]
	private void ToggleCam( bool toggle )
	{
		var id = Components.Get<CameraComponent>( true );
		id.IsMainCamera = toggle;
		id.Enabled = toggle;
	}

	[Broadcast]
	private void PlayStart()
	{
		Sound.Play( "sounds/game/ringsoundevent.sound" );
	}

	/// <summary>
	/// Initializes the game (seeker timeout, logic firing and stuff).
	/// </summary>
	private async void InitGame()
	{
		if ( Networking.Connections.Count <= 1 )
		{
			ToggleCam( true );
			GameBegan = false;
			return;
		}
		ToggleCam( false );

		OnRoundStart?.Invoke();
		InitRound();
		Log.Info( "Initiating the game." );
		CurrentRound.StartTheRound();

		//Giving hiders time to hide
		ToggleSeekers( false );
		Log.Info( "Seekers disabled." );
		await Task.DelayRealtimeSeconds( PreparationTime );
		ToggleSeekers( true );
		PlayStart();
		Log.Info( "Seekers enabled." );
	}

	/// <summary>
	/// Round initialization.
	/// </summary>
	private void InitRound()
	{
		CurrentRound = new( RoundLength );
		OnTeamLost += LogTeamLost;
		Seekers = new( "Seekers", "red" );
		Hiders = new( "Hiders", "blue" );
		CurrentRound.Start += StartRound;
		CurrentRound.End += EndRound;
	}

	/// <summary>
	/// Checks player count in each team.
	/// </summary>
	/// <returns>Returns the team that has lost the round. Otherwise returns null.</returns>
	private Team LostTeam()
	{

		if ( Seekers.TeamPlayers.Count <= 0 ) return Seekers;
		if ( Hiders.TeamPlayers.Count <= 0 ) return Hiders;

		return null;
	}

	/// <summary>
	/// Assigns players with roles and spawns pawns for them.
	/// </summary>
	private void StartRound()
	{
		if ( IsProxy ) { return; }

		//Selecting seekers
		int seekersCount = Math.Max( Networking.Connections.Count / 4, 1 );
		int selected = 0;
		int[] seekersIndexes = new int[seekersCount];
		seekersIndexes[0] = -1;
		while ( selected < seekersCount )
		{
			int index = Game.Random.Next( Networking.Connections.Count );
			var connection = Networking.Connections.ElementAt( index );
			Guid connId = connection.Id;

			if ( seekersIndexes.Contains( index ) )
				continue;

			Seekers?.TeamPlayers.Add( connection.Id );
			RespawnPlayer( connId );
			seekersIndexes[selected] = index;
			selected++;
		}

		//assigning the remaining players to hiders team
		for ( int i = 0; i < Networking.Connections.Count; i++ )
		{
			if ( !seekersIndexes.Contains( i ) )
			{
				// Spawn this object and make the client the owner
				RespawnPlayer( Networking.Connections[i].Id, "hiders" );
				Hiders?.TeamPlayers.Add( Networking.Connections[i].Id );
			}
		}
	}

	/// <summary>
	/// Respawn player by providing them a new pawn to play as.
	/// </summary>
	/// <param name="connection">Player connection.</param>
	/// <param name="tag">Player team.</param>
	/// <returns>Pawn GameObject.</returns>
	private void RespawnPlayer( Guid connection, string tag = "seekers" )
	{
		var startLocation = NetworkComponent.FindSpawnLocation().WithScale( 1 );
		var conn = Networking.Connections.Where( x => x.Id == connection ).First();
		// Spawn this object and make the client the owner
		var player = NetworkComponent.PlayerPrefab.Clone( startLocation, name: $"Player - {conn.DisplayName}" );
		player.Tags.Add( tag );
		var ui = PawnUIPrefab.Clone( player, player.Transform.Position, player.Transform.Rotation, player.Transform.Scale );
		//ui.NetworkSpawn( conn );
		PlayerPawns.Add( player.Id );
		if ( tag == "seekers" )
		{
		}

		player.NetworkSpawn( conn );
	}

	/// <summary>
	/// Invokes pawn removal and next round start.
	/// </summary>
	private async void EndRound()
	{
		await Task.DelayRealtimeSeconds( RoundCooldown );

		RemoveAllPawns();
		InitGame();
	}

	/// <summary>
	/// Clears the scene from every player pawn.
	/// </summary>
	[Broadcast]
	private void RemoveAllPawns()
	{
		for ( int i = 0; i < PlayerPawns.Count; i++ )
		{
			var gameObject = Scene.Directory.FindByGuid( PlayerPawns[i] );
			gameObject?.Destroy();
		}
		PlayerPawns.Clear();
	}
	#endregion
}
