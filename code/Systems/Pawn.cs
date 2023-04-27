using HideAndSeek.Systems.Controllers;
using HideAndSeek.Systems.Controllers.Movement;
using Sandbox;

namespace HideAndSeek;

public partial class Pawn : AnimatedEntity
{
	/// <summary>
	/// Called when the entity is first created 
	/// </summary>

	public bool ThirdPerson { get; set; } = true;
	public float GroundAngle { get; set; } = 46f;
	public float StepSize { get; set; } = 18f;
	[BindComponent] public CameraController PawnCamera { get; }
	[BindComponent] public PawnAnimator PawnAnimations { get; }
	[BindComponent] public MainController Controller { get; }
	[BindComponent] public WalkingController Walking { get; }


	public override void Spawn()
	{
		SetModel( "models/citizen/citizen.vmdl" );


		Predictable = true;
		EnableDrawing = true;
		EnableHideInFirstPerson = true;
		EnableShadowInFirstPerson = true;
		EnableLagCompensation = true;
		EnableHitboxes = true;
		Tags.Add( "player" );
	}

	public void Respawn()
	{
		SetupPhysicsFromAABB( PhysicsMotionType.Keyframed, new Vector3( 16, 16, 0 ), new Vector3( 16, 16, 72 ) );

		EnableAllCollisions = true;
		EnableHitboxes = true;
		EnableShadowCasting = true;


		Components.Create<MainController>();

		Components.RemoveAny<MechanicBaseClass>();

		Components.Create<WalkingController>();
		Components.Create<AirMove>();

		Components.Create<CameraController>();
		Components.Create<PawnAnimator>();

		GameManager.Current?.MoveToSpawnpoint( this );
		ResetInterpolation();
		DressUp(Client);
	}

	private void DressUp( IClient client )
	{
		ClothingContainer clothes = new();
		clothes.LoadFromClient( client );
		clothes.DressEntity( this );
	}

	/// <summary>
	/// Called every tick, clientside and serverside.
	/// </summary>
	public override void Simulate( IClient client )
	{

		Rotation = ViewAngles.WithPitch( 0f ).ToRotation();


		LocalEyePosition = Vector3.Up * (64f * Scale);
		EyeRotation = ViewAngles.ToRotation();


		Controller?.Simulate( client );
		PawnAnimations?.Simulate( client );

		// If we're running serverside and Attack1 was just pressed, spawn a ragdoll
		if ( Game.IsServer && Input.Pressed( "attack1" ) )
		{
			var ragdoll = new ModelEntity();
			ragdoll.SetModel( "models/citizen/citizen.vmdl" );
			ragdoll.Position = Position + Rotation.Forward * 40;
			ragdoll.Rotation = Rotation.LookAt( Vector3.Random.Normal );
			ragdoll.SetupPhysicsFromModel( PhysicsMotionType.Dynamic, false );
			ragdoll.PhysicsGroup.Velocity = Rotation.Forward * 1000;
		}
	}

	/// <summary>
	/// Called every frame on the client
	/// </summary>
	public override void FrameSimulate( IClient cl )
	{
		DebugOverlay.ScreenText( Position.ToString(), 1 );
		DebugOverlay.ScreenText( CollisionBounds.ToString(), 2 );
		DebugOverlay.ScreenText( Time.Delta.ToString(), 3 );
		DebugOverlay.ScreenText( Velocity.ToString(), 4 );
		PawnCamera?.Update( this );

		/*// Simulate rotation every frame, to keep things smooth
		Rotation = ViewAngles.ToRotation();

		PawnCamera.Position = Position;
		PawnCamera.Rotation = Rotation;

		// Set field of view to whatever the user chose in options
		PawnCamera.FieldOfView = Screen.CreateVerticalFieldOfView( Game.Preferences.FieldOfView );

		// Set the first person viewer to this, so it won't render our model
		PawnCamera.FirstPersonViewer = this;*/
	}
}
