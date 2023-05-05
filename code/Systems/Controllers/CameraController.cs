using Sandbox;

namespace HideAndSeek;

public partial class CameraController : EntityComponent<Pawn>, ISingletonComponent
{
	private Vector3 _viewPosition { get; set; }
	private Rotation PrevRotation { get; set; }
	private TimeSince SinceRotation { get; set; }


	public void Update( Pawn pawn )
	{
		PrevRotation = Camera.Rotation;
		Camera.Rotation = pawn.ViewAngles.ToRotation();
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

		if ( pawn.ThirdPerson )
		{
			Vector3 targetPosition;
			Rotation viewRotation = Camera.Rotation;

			_viewPosition = _viewPosition.LerpTo( pawn.Position + Vector3.Up * 64, Time.Delta * 8 );



			targetPosition = _viewPosition;// + viewRotation.Right * ((CollisionBounds.Mins.x + 50) * Scale);
			float distance = 80.0f * pawn.Scale;
			targetPosition += viewRotation.Forward * -distance;


			TraceResult rayTrace = Trace.Ray( _viewPosition, targetPosition )
				.WithAnyTags( "solid" )
				.Ignore( pawn )
				.Radius( 8 )
				.Run();


			Camera.FirstPersonViewer = null;

			Camera.Position = rayTrace.EndPosition;

			//DebugOverlay.ScreenText( Camera.Rotation.Angles().ToString(), 10 );
		}
		else
		{
			Camera.FirstPersonViewer = pawn;
			Camera.Position = pawn.EyePosition;
		}
	}
}
