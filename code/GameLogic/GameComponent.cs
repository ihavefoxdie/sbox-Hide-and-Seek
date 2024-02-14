using Sandbox;
using Sandbox.GameLogic.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
namespace HideAndSeek;


public class GameComponent : Component, Component.INetworkListener
{
	#region Properties
	public Round CurrentRound { get; set; }
	[Property] public List<Team> Teams { get; set; }
	//[Property] public List<GameObject> Players { get; set; }
	[Property] public int RoundCooldown { get; set; }
	[Property] public NetworkComponent NetworkComponent { get; set; }
	private bool GameBegan { get; set; } = false;
	/// <summary>
	/// Round length limit in seconds.
	/// </summary>
	[Property] public int RoundLength { get; set; } = 300;
	#endregion


	#region Actions and Events
	public Action<Team> OnTeamLost { get; set; }
	#endregion


	protected override void OnAwake()
	{
		base.OnStart();
	}

	public void OnActive( Connection conn )
	{
		if ( CurrentRound != null )
		{
			if ( GameBegan )
				return;
		}

		if ( Networking.Connections.Count > 0 )
		{
			GameBegan = true;
			InitGame();
		}
	}

	public void OnConnected( Connection conn )
	{
/*		if ( CurrentRound != null )
		{
			if(GameBegan)
				return;
		}

		GameBegan = true;
		InitGame();*/
	}

	public void OnDisconnected( Connection conn )
	{

	}

	protected override void OnEnabled()
	{
		base.OnEnabled();
	}

	protected override void OnUpdate()
	{
		if(CurrentRound is null || Teams is null ) return;
		if ( CurrentRound.IsStarted )
		{
			Team lost = LostTeam();
			if ( lost != null )
			{
				OnTeamLost?.Invoke( lost );
			}
			CurrentRound.CheckRoundTime();
		}
	}


	#region Methods
	public void InitGame()
	{
		InitRound();
		Log.Info( "Initiating the game." );
		CurrentRound.StartTheRound();
	}

	private void InitRound()
	{
		CurrentRound = new( RoundLength );
		Teams = new List<Team>
		{
			new ( "Seekers", "red" ),
			new ( "Hiders", "blue" )
		};
		CurrentRound.Start += StartRound;
		CurrentRound.End += EndRound;
	}

	/// <summary>
	/// Checks player count in each team.
	/// </summary>
	/// <returns>Returns the team that has lost the round. Otherwise returns null.</returns>
	private Team? LostTeam()
	{
		for ( int i = 0; i < Teams.Count; i++ )
		{
			if ( Teams[i].TeamPlayers.Count <= 0 )
				return Teams[i];
		}

		return null;
	}

	private void StartRound()
	{
		if ( IsProxy ) { return; }

		//Selecting seekers
		int seekersCount = Math.Max( Networking.Connections.Count / 4, 1 );
		int selected = 0;
		int[] seekersIndexes = new int[ seekersCount ];
		seekersIndexes[0] = -1;
		while ( selected < seekersCount )
		{
			int index = new Random().Next( seekersCount - 1 );
			var connection = Networking.Connections.ElementAt( index );

			if ( seekersIndexes.Contains(index) )
				continue;

			Teams.Find( x => x.Name == "Seekers" ).TeamPlayers.Add( connection );
			RespawnPlayer( connection ).Tags.Add( "Seekers" );
			seekersIndexes[ selected ] = index;
			selected++;
		}

		//assigning the remaining players to hiders team
		for ( int i = 0; i < Networking.Connections.Count; i++ )
		{
			if(!seekersIndexes.Contains(i))
			{
				var startLocation = NetworkComponent.FindSpawnLocation().WithScale( 1 );

				// Spawn this object and make the client the owner
				RespawnPlayer( Networking.Connections[i] ).Tags.Add( "Hiders" );
				Teams.Find( x => x.Name == "Hiders" ).TeamPlayers.Add( Networking.Connections[i] );
			}
		}

		//Respawn();
	}

	private GameObject RespawnPlayer(Connection connection)
	{
		var startLocation = NetworkComponent.FindSpawnLocation().WithScale( 1 );

		// Spawn this object and make the client the owner
		var player = NetworkComponent.PlayerPrefab.Clone( startLocation, name: $"Player - {connection.DisplayName}" );
		player.NetworkSpawn( connection );

		return player;
	}

	private async void EndRound()
	{
		await Task.DelayRealtimeSeconds( RoundCooldown );
		RemoveAllPawns();
		await Task.DelayRealtimeSeconds( 1f );
		InitGame();
	}

	[Broadcast]
	private void RemoveAllPawns()
	{
		var pawns = Scene.GetAllObjects( false ).Where( x => x.Name.Contains( "Player" ) );
		foreach ( var player in pawns )
		{
			//player.Network.DropOwnership();
			player.Destroy();
		}
	}

	/// <summary>
	/// Try to join selected team.
	/// </summary>
	/// <param name="team">Selected team.</param>
	/// <param name="conn">Client connection.</param>
	/// <returns>Returns true on a successful join, false on a failed one.</returns>
/*	public bool JoinTeam( int team, Connection conn )
	{
		if ( team < 0 || team >= Teams.Count ) return false;

		for ( int i = 0; i < Teams.Count; i++ )
		{
			if ( Teams[team].TeamPlayers.Count > Teams[i].TeamPlayers.Count )
				return false;
		}
		var pawn = Players.Find( x => x.Name.Contains( conn.DisplayName ) );
		pawn.Tags.Add( Teams[team].Name );
		Teams[team].TeamPlayers.Add( pawn );
		return true;
	}*/

	#endregion
}
