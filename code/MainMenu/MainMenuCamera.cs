using Sandbox;

public class MainMenuCamera : Component
{
	private CameraComponent _camera;
	bool reverse = false;

	protected override void OnStart()
	{
		base.OnStart();

		_camera = Components.Get<CameraComponent>();
		_camera.Transform.LocalRotation = _camera.Transform.LocalRotation.Angles().WithYaw( -90 );
	}

	protected override void OnFixedUpdate()
	{
		if ( _camera.Transform.LocalRotation.Yaw() >= -90 && _camera.Transform.LocalRotation.Yaw() <= -80  && !reverse)
		{
			_camera.Transform.LocalRotation = Rotation.Lerp( _camera.Transform.LocalRotation, _camera.Transform.LocalRotation.Angles().WithYaw( -75 ), Time.Delta * 0.015f );
		}
		else
		{
			reverse = true;
			_camera.Transform.LocalRotation = Rotation.Lerp( _camera.Transform.LocalRotation, _camera.Transform.LocalRotation.Angles().WithYaw( -90 ), Time.Delta * 0.015f );
			if( _camera.Transform.LocalRotation.Yaw() <= -88 )
				reverse = false;
		}
	}
}
