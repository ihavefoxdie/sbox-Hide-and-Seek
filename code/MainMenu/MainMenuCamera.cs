using Sandbox;

public class MainMenuCamera : Component
{
	private CameraComponent _camera;
	bool reverse = false;

	protected override void OnStart()
	{
		base.OnStart();

		_camera = Components.Get<CameraComponent>();
		_camera.Transform.LocalRotation = _camera.Transform.LocalRotation.Angles().WithYaw( -70 );
	}

	protected override void OnFixedUpdate()
	{
		if ( _camera.Transform.LocalRotation.Yaw() >= -70 && _camera.Transform.LocalRotation.Yaw() <= -50  && !reverse)
		{
			_camera.Transform.LocalRotation = Rotation.Lerp( _camera.Transform.LocalRotation, _camera.Transform.LocalRotation.Angles().WithYaw( -15 ), Time.Delta * 0.015f );
		}
		else
		{
			reverse = true;
			_camera.Transform.LocalRotation = Rotation.Lerp( _camera.Transform.LocalRotation, _camera.Transform.LocalRotation.Angles().WithYaw( -70 ), Time.Delta * 0.015f );
			if( _camera.Transform.LocalRotation.Yaw() <= -68 )
				reverse = false;
		}
	}
}
