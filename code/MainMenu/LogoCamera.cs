using Sandbox;

namespace HideAndSeek;

public class LogoCamera : Component
{
	private CameraComponent _camera;
	private LogoComponent _logo;
	private Texture _logoTexture;

	protected override void OnStart()
	{
		_camera = Components.Get<CameraComponent>();
		_logo = Components.GetInParent<LogoComponent>();
		_logoTexture = _logo.LogoTexture;
	}

	protected override void OnFixedUpdate()
	{
		_camera.RenderToTexture( _logoTexture );
	}
}
