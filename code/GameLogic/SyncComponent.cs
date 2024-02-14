using Sandbox;
using System.Linq;
namespace HideAndSeek;

public class SyncComponent : Component, Component.INetworkListener
{
	[Property] public GameComponent CurrentGame { get; set; }
	[Property] [Sync] public float Timer { get; set; }

	protected override void OnAwake()
	{
		var fuck = Scene.GetAllComponents<GameComponent>();
		CurrentGame = fuck.Last();
	}

	protected override void OnUpdate()
	{
		if ( IsProxy )
		{
			return;
		}
		Timer = CurrentGame.CurrentRound.TimeSinceStart.Relative;
	}
}
