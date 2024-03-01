using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HideAndSeek;

public class SyncComponent : Component, Component.INetworkListener
{
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
	[Sync] public int MaxTime {  get; private set; }
	[Sync] public int RoundCooldown { get; private set; }
	[Sync] public int PreparationTime { get; private set; }

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
	}

	protected override void OnFixedUpdate()
	{
		if ( IsProxy )
		{
			return;
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
	[Broadcast]
	public void OnCaught(Guid catcher, Guid caught)
	{
		if(!IsProxy)
			CurrentGame.Caught( catcher, caught );
	}
	#endregion
}
