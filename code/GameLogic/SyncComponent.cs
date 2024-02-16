using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
namespace HideAndSeek;

public class SyncComponent : Component, Component.INetworkListener
{
	[Property] public GameComponent CurrentGame { get; set; }
	[Property][Sync] public float Timer { get; set; }
	[Property][Sync] public List<Guid> Hiders { get; set; }
	[Property][Sync] public List<Guid> Seekers { get; set; }

	protected override void OnAwake()
	{
		var fuck = Scene.GetAllComponents<GameComponent>();
		CurrentGame = fuck.Last();
		Hiders = new();
		Seekers = new();
	}

	protected override void OnUpdate()
	{
		if ( IsProxy )
		{
			return;
		}
		if ( CurrentGame is not null && CurrentGame.CurrentRound is not null )
		{
			Timer = CurrentGame.CurrentRound.TimeSinceStart.Relative;
			if ( CurrentGame.Seekers is not null && CurrentGame.Hiders is not null )
			{
				Seekers = CurrentGame.Seekers.TeamPlayers.ToList();
				Hiders = CurrentGame.Hiders.TeamPlayers.ToList();
			}
		}
	}
}
