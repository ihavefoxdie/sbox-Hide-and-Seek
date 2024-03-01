using Sandbox;

public class MainMenuCamera : Component
{
	private CameraComponent _camera;
	bool reverse = false;

	protected override void OnStart()
	{
		base.OnStart();

		_camera = Components.Get<CameraComponent>();
		_camera.Transform.LocalRotation = _camera.Transform.LocalRotation.Angles().WithYaw( -160 );
	}

	protected override void OnUpdate()
	{
		if ( _camera.Transform.LocalRotation.Yaw() >= -160 && _camera.Transform.LocalRotation.Yaw() <= -106  && !reverse)
		{
			_camera.Transform.LocalRotation = Rotation.Lerp( _camera.Transform.LocalRotation, _camera.Transform.LocalRotation.Angles().WithYaw( -105 ), Time.Delta * 0.015f );
		}
		else
		{
			reverse = true;
			_camera.Transform.LocalRotation = Rotation.Lerp( _camera.Transform.LocalRotation, _camera.Transform.LocalRotation.Angles().WithYaw( -160 ), Time.Delta * 0.015f );
			if( _camera.Transform.LocalRotation.Yaw() <= -158 )
				reverse = false;
		}
	}
}
