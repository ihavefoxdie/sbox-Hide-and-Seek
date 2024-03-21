using Sandbox;
namespace HideAndSeek;

public class TestingBug : Component
{
	protected override void OnFixedUpdate()
	{
		base.OnFixedUpdate();
		Log.Info( this.GameObject.Id );
	}
}
