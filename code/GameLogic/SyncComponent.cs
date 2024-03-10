using Sandbox;
using Sandbox.Network;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HideAndSeek;

public class SyncComponent : Component, Component.INetworkListener
{
	#region Properties
	/// <summary>
	/// Current GameComponent instance.
	/// </summary>
	[Property] public GameComponent CurrentGame { get; set; }
	[Property] public SettingsLoaderComponent SettingsData { get; private set; }
	/// <summary>
	/// Game's timer.
	/// </summary>
	[Property][Sync] public float Timer { get; private set; }
	[Property][Sync] public string MapIdent { get; private set; }
	[Sync] private Guid GameHostConnection { get; set; }
	[Sync] public int MaxTime { get; private set; }
	[Sync] public int RoundCooldown { get; private set; }
	[Sync] public int PreparationTime { get; private set; }
	[Sync] public bool DisconnectTriggered { get; set; } = false;
	private int HostSentMessages { get; set; } = 0;
	private TimeSince SinceLastMessage { get; set; } = 0;
	/// <summary>
	/// Has the game started yet.
	/// </summary>
	[Property][Sync] public bool IsStarted { get; private set; }
	/// <summary>
	/// List of connections in Hiders team.
	/// </summary>
	[Property][Sync] public List<Guid> Hiders { get; private set; }
	/// <summary>
	/// List of connections in Seekers team.
	/// </summary>
	[Property][Sync] public List<Guid> Seekers { get; private set; }
	#endregion

	#region Events
	public event Action<string> SystemMessage;
	#endregion
	protected override void OnAwake()
	{
		CurrentGame = Scene.GetAllComponents<GameComponent>().Last();
		SettingsData = Scene.GetAllComponents<SettingsLoaderComponent>().Last();
		Hiders = new();
		Seekers = new();
		MaxTime = CurrentGame.RoundLength;
		RoundCooldown = CurrentGame.RoundCooldown;
		PreparationTime = CurrentGame.PreparationTime;
		MapIdent = SettingsData.MapIdent;
		GameHostConnection = Networking.HostConnection.Id;
	}

	protected override void OnFixedUpdate()
	{
		if ( IsProxy )
		{
			if ( HostSentMessages != Networking.HostConnection.MessagesSent )
			{
				HostSentMessages = Networking.HostConnection.MessagesSent;
				SinceLastMessage = 0;
			}

			if ( SinceLastMessage.Relative == 5 )
			{
				SystemMessage?.Invoke( "Host timeout" );
			}
			if ( SinceLastMessage.Relative > 10 )
			{
				Disconnect();
			}

			return;
		}

		if ( (Networking.HostConnection == null || GameHostConnection != Networking.HostConnection?.Id) && !DisconnectTriggered )
		{
			DisconnectTriggered = true;
			DisconnectEveryone( "Host has left!" );
		}

		if ( CurrentGame is not null && CurrentGame.CurrentRound is not null )
		{
			IsStarted = CurrentGame.GameBegan;
			Timer = CurrentGame.CurrentRound.TimeSinceStart.Relative;
			if ( CurrentGame.Seekers is not null && CurrentGame.Hiders is not null )
			{
				Seekers = CurrentGame.Seekers.TeamPlayers.ToList();
				Hiders = CurrentGame.Hiders.TeamPlayers.ToList();
			}
		}
	}

	#region Methods
	public static void Disconnect()
	{
		GameNetworkSystem.Disconnect();
		Game.ActiveScene.LoadFromFile( "scenes/main_menu.scene" );
	}

	public static void Quit()
	{
		GameNetworkSystem.Disconnect();
		Game.ActiveScene.Destroy();
		Game.Close();
	}

	[Broadcast]
	private void DisconnectEveryone( string message )
	{
		SystemMessage?.Invoke( message );
		Log.Info(message);
		Disconnect();
	}

	[Broadcast]
	public void OnCaught( Guid catcher, Guid caught )
	{
		if ( !IsProxy )
			CurrentGame.Caught( catcher, caught );
	}
	#endregion
}
