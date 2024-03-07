using Sandbox;

namespace HideAndSeek;

public class SceneCameraComponent : Component
{
	[Property] private CameraComponent Camera { get; set; }
	[Property] private NetworkComponent Networking { get; set; }

	protected override void OnStart()
	{
		base.OnStart();
		if ( Camera != null && Networking != null)
		{
			Camera.Transform.World = Networking.FindSpawnLocation().WithScale( 1 );
		}
	}

	protected override void OnUpdate()
	{

	}
}
