using Sandbox;

namespace HideAndSeek;

public class SceneCameraComponent : Component
{
	[Property] private CameraComponent Camera { get; set; }
	[Property] private NetworkComponent Networking { get; set; }
	private bool Changed = false;

	protected override void OnStart()
	{
		base.OnStart();
	}

	protected override void OnUpdate()
	{
		if ( !Changed )
			ChangeSpot();
	}

	private async void ChangeSpot()
	{
		if ( Camera != null && Networking != null )
		{
			Transform newPos = Networking.FindSpawnLocation().WithScale( 1 );
			Camera.Transform.World = newPos.Add(new Vector3(0, 0, 50), true);
			Changed = true;
		}
		await Task.DelayRealtimeSeconds( 5 );
		Changed = false;
	}
}
