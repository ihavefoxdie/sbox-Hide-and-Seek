using Sandbox;

namespace HideAndSeek;

public partial class CameraController : EntityComponent<Pawn>, ISingletonComponent
{
	public Vector3 ViewPosition { get; set; }


	public void Update( Pawn pawn )
	{
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
			float distance = 80.0f * pawn.Scale;

			ViewPosition = ViewPosition.LerpTo( pawn.Position + Vector3.Up * 64, Time.Delta * 8 );
			targetPosition = ViewPosition;// + viewRotation.Right * ((CollisionBounds.Mins.x + 50) * Scale);
			targetPosition += viewRotation.Forward * -distance;

			TraceResult rayTrace = Trace.Ray( ViewPosition, targetPosition )
				.WithAnyTags( "solid" )
				.Ignore( pawn )
				.Radius( 8 )
				.Run();


			Camera.FirstPersonViewer = null;
			Camera.Position = rayTrace.EndPosition;
		}
		else
		{
			Camera.FirstPersonViewer = pawn;
			Camera.Position = pawn.EyePosition;
		}
	}
}
