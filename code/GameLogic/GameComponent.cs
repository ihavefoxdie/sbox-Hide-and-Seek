using Sandbox;
using Sandbox.GameLogic.Modules;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
namespace HideAndSeek;


public class GameComponent : Component, Component.INetworkListener
{
	#region Properties
	[Property] public Round CurrentRound { get; set; }
	[Property] public List<Team> Teams { get; set; }
	[Property] public List<GameObject> Players { get; set; }
	[Property] public TimeSince TimeSinceStart { get; set; }
	/// <summary>
	/// Round length in seconds
	/// </summary>
	[Property] public int RoundLength { get; set; } = 300;
	#endregion



	protected override void OnAwake()
	{
		base.OnStart();
		CurrentRound = new( RoundLength );
		Teams = new List<Team>
		{
			new ( "Seekers", "red" ),
			new ( "Hiders", "blue" )
		};
		CurrentRound.Start = Respawn;
	}

	public void OnActive( Connection conn )
	{
		var player = Scene.GetAllObjects( false ).Where( x => x.Name.Contains( conn.DisplayName ) );
		Players.AddRange( player );

		if ( Teams[0].TeamPlayers.Count > Teams[1].TeamPlayers.Count )
		{
			Teams[1].TeamPlayers.AddRange( player );
		}
		else if ( Teams[0].TeamPlayers.Count < Teams[1].TeamPlayers.Count )
		{
			Teams[0].TeamPlayers.AddRange( player );
		}
		else
		{
			Teams[0].TeamPlayers.AddRange( player );
		}
	}

	public void OnConnected( Connection conn )
	{

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
		if ( CurrentRound.IsStarted == false)
		{
			CurrentRound.StartTheRound();
		}
		CurrentRound.CheckRoundTime();
	}


	#region Methods
	private void Respawn()
	{
		foreach ( var player in Players )
		{
			var component = player.Components.Get<PawnComponent>();
			component.Spawn();
		}
	}

	private void EndRound()
	{
		CurrentRound.StartTheRound();
	}
	#endregion
}
