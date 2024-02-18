using Sandbox;
using Sandbox.Citizen;
using Sandbox.GameLogic.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
namespace HideAndSeek;


public class GameComponent : Component, Component.INetworkListener
{
	#region Properties
	public Round CurrentRound { get; set; }
	//public List<Team> Teams { get; set; }
	public Team Hiders { get; set; }
	public Team Seekers { get; set; }
	[Sync] public List<Guid> PlayerPawns { get; set; }
	[Property] public NetworkComponent NetworkComponent { get; set; }
	[Sync] public bool GameBegan { get; private set; } = false;
	private CameraComponent Camera { get; set; }
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
	[Property] public int PreparationTime { get; set; } = 10;
	#endregion


	#region Actions and Events
	public Action<Team> OnTeamLost { get; set; }
	public Action OnRoundStart { get; set; }
	#endregion


	protected override void OnAwake()
	{
		PlayerPawns = new();
	}
	protected override void OnStart()
	{
		Camera = Components.Get<CameraComponent>();
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


	#region Methods
	private void LogTeamLost( Team team )
	{
		if ( team != null )
			Log.Info( team.Name + " team has lost!" );
	}

	private void ToggleSeekers( bool toggle )
	{
		var pawns = Scene.GetAllObjects( false ).Where( x => x.Tags.Has( "seekers" ) );
		foreach ( var pawn in pawns )
		{
			TogglePawn( pawn.Id, toggle );
		}
	}

	[Broadcast]
	private void TogglePawn( Guid guid, bool toggle )
	{
		var pawn = Scene.Directory.FindByGuid( guid );
		if ( pawn != null )
		{
			pawn.Enabled = toggle;
			if ( toggle == true )
			{
				pawn.Components.Create<TeamEquipmentComponent>( true );
			}
		}
	}

	[Broadcast]
	private void ToggleCam(bool toggle)
	{
		var id = Components.Get<CameraComponent>(true);
		id.IsMainCamera = toggle;
		id.Enabled = toggle;
	}

	private async void InitGame()
	{
		if ( Networking.Connections.Count <= 1 )
		{
			ToggleCam( true);
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
		Log.Info( "Seekers enabled." );
	}

	private void InitRound()
	{
		CurrentRound = new( RoundLength );
		OnTeamLost = LogTeamLost;
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
			int index = new Random().Next( Networking.Connections.Count - 1 );
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
				Hiders?.TeamPlayers.Add(Networking.Connections[i].Id);
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
		PlayerPawns.Add( player.Id );
		if ( tag == "seekers" )
		{
		}

		player.NetworkSpawn( conn );
	}

	private async void EndRound()
	{
		await Task.DelayRealtimeSeconds( RoundCooldown );
		RemoveAllPawns();
		InitGame();
	}

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
