using Sandbox;
using System;

namespace HideAndSeek;

public partial class CameraController : EntityComponent<Pawn>, ISingletonComponent
{
	private Vector3 _viewPosition { get; set; }


	public void Update( IClient client )
	{
		Camera.Rotation = Entity.ViewAngles.ToRotation();
		Camera.FieldOfView = Screen.CreateVerticalFieldOfView( Game.Preferences.FieldOfView );

		var pp = Camera.Main.FindOrCreateHook<Sandbox.Effects.ScreenEffects>();
		pp.Vignette.Intensity = 0.60f;
		pp.Vignette.Roundness = 1f;
		pp.Vignette.Smoothness = 0.3f;
		pp.Vignette.Color = Color.Black.WithAlpha( 1f );
		pp.MotionBlur.Scale = 0f;
		pp.Saturation = 1f;
		pp.FilmGrain.Response = 1f;
		pp.FilmGrain.Intensity = 0.01f;

		if ( Entity.ThirdPerson )
		{
			Vector3 targetPosition;
			Rotation viewRotation = Camera.Rotation;

			_viewPosition = _viewPosition.LerpTo( Entity.EyePosition, Time.Delta * 8 );



			targetPosition = _viewPosition;// + viewRotation.Right * ((CollisionBounds.Mins.x + 50) * Scale);
			float distance = 80.0f * Entity.Scale;
			targetPosition += viewRotation.Forward * -distance;


			TraceResult rayTrace = Trace.Ray( _viewPosition, targetPosition )
				.WithAnyTags( "solid" )
				.Ignore( Entity )
				.Radius( 8 )
				.Run();


			Camera.FirstPersonViewer = null;

			DebugOverlay.ScreenText( Vector3.DistanceBetween( Entity.Position, rayTrace.EndPosition ).ToString(), 10 );
			if ( Vector3.DistanceBetween( Entity.Position, rayTrace.EndPosition ) < 40 )
				Entity.EnableDrawing = false;
			else
				Entity.EnableDrawing = true;

			Camera.Position = rayTrace.EndPosition;

			//DebugOverlay.ScreenText( Camera.Rotation.Angles().ToString(), 10 );
		}
		else
		{
			Camera.FirstPersonViewer = Entity;
			Camera.Position = Entity.EyePosition;
		}
	}
}
