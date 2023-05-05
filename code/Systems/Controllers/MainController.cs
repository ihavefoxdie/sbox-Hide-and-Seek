using HideAndSeek.Systems.Controllers;
using HideAndSeek.Systems.Controllers.Movement;
using Sandbox;

namespace HideAndSeek;

public partial class MainController : EntityComponent<Pawn>, ISingletonComponent
{
	//We need to setup basic stuff like collisions and such
	[Net, Predicted]
	public float CurrentEyeHeight { get; set; } = 64f;
	public float MoveScale { get; set; } = 1f;
	public float CurrentGroundAngle { get; set; }
	protected float _bodyGirth = 32f;
	public Vector3 GroundNormal { get; set; }
	public Vector3 Mins { get; set; }
	public Vector3 Maxs { get; set; }

	public float MaxGroundVelocity { get; private set; } = 150f;
	public float StepSize { get; private set; } = 12f;
	public float GroundAngle { get; private set; } = 45f;
	public float SurfaceFriciton { get; set; } = 1f;
	public Entity GroundEntity { get; set; }

	public MechanicBase CurrentMechanic;
	public MechanicFactory Mechanics;



	public MainController() : base()
	{
		GroundEntity = null;
		Mechanics = new MechanicFactory( this );
		CurrentMechanic = Mechanics.Gravity();
	}

	public Pawn ThisPawn
	{
		get { return Entity; }
	}

	private void ExecuteMechanics()
	{
		if ( CurrentMechanic != null )
		{
			CurrentMechanic.SimulateMechanics();
		}
		else
			DebugOverlay.ScreenText( "NULL", 10 );
	}

	public void Simulate( IClient client )
	{
		CategorizePosition( GroundEntity != null );
		ExecuteMechanics();
		if ( GroundEntity != null )
			DebugOverlay.ScreenText( GroundEntity.ToString(), 10 );
	}



	#region Ground Related Logic
	public void SetGroundEntity( Entity entity )
	{
		GroundEntity = entity;
		if ( GroundEntity != null )
		{
			ThisPawn.Velocity = ThisPawn.Velocity.WithZ( 0 );
			ThisPawn.BaseVelocity = GroundEntity.Velocity;
		}
	}

	public void CategorizePosition( bool stayOnGround )
	{
		SurfaceFriciton = 1f;

		Vector3 point = ThisPawn.Position - Vector3.Up * 2;
		Vector3 bumpOrigin = ThisPawn.Position;
		bool rapidlyMovingUp = ThisPawn.Velocity.z > MaxGroundVelocity;
		bool moveToEndPosition = false;

		//if we leave the ground, there's no need to process anything else
		if ( rapidlyMovingUp )
		{
			ClearGorundEntity();
			return;
		}

		if ( GroundEntity != null || stayOnGround )
		{
			moveToEndPosition = true;
			point.z -= StepSize;
		}

		TraceResult trace = TraceBBox( bumpOrigin, point, 4.0f );

		float angle = Vector3.GetAngle( Vector3.Up, trace.Normal );
		CurrentGroundAngle = angle;

		if ( trace.Entity == null || angle > GroundAngle )
		{
			ClearGorundEntity();
			moveToEndPosition = false;

			if ( ThisPawn.Velocity.z > 0 )
				SurfaceFriciton = 0.25f;
		}
		else
		{
			CheckGoundEntity( trace );
		}

		if ( moveToEndPosition && !trace.StartedSolid && trace.Fraction > 0f && trace.Fraction < 1f )
		{
			ThisPawn.Position = trace.EndPosition;
		}
	}

	public void ClearGorundEntity()
	{
		if ( GroundEntity == null ) return;

		GroundEntity = null;
		SurfaceFriciton = 1.0f;
	}

	//This method determines the ground entity. It also can tell us if there is no ground at all
	private void CheckGoundEntity( TraceResult trace )
	{
		GroundNormal = trace.Normal;

		SurfaceFriciton = trace.Surface.Friction * 1.25f;
		if ( SurfaceFriciton > 1f ) SurfaceFriciton = 1f;

		SetGroundEntity( trace.Entity );
	}
	#endregion



	#region Movement related code
	public void Accelerate( Vector3 desiredDirection, float desiredSpeed, float speedLimit, float acceleration )
	{
		ThisPawn.Velocity = PawnMovementPhysics.CalculateAcceleration( ThisPawn.Velocity, desiredDirection, desiredSpeed, speedLimit, acceleration );
	}

	public void ApplyFriction( float stopSpeed, float friction = 1f )
	{
		ThisPawn.Velocity = PawnMovementPhysics.Friction( ThisPawn.Velocity, stopSpeed, friction );
	}

	public Vector3 GetInputVelocity( bool zeroPitch = false )
	{
		Vector3 result = new( ThisPawn.InputDirection.x, ThisPawn.InputDirection.y, 0 );
		result *= MoveScale;

		Vector3 inMovement = result.Length.Clamp( 0f, 1f );

		result *= ThisPawn.ViewAngles.WithPitch( 0f ).ToRotation();

		if ( zeroPitch ) result.z = 0f;

		result = result.Normal * inMovement;
		result *= GetDesiredSpeed();

		return result *= CurrentGroundAngle.Remap( 0, 45, 1, 0.6f );
	}


	public virtual float GetDesiredSpeed()
	{
		return CurrentMechanic?.DesiredSpeed ?? 180f;
	}

	//Simple Move
	public void Move()
	{
		ProcessMoveHelper( out MoveHelper helper, out float stepSize );

		if ( helper.TryMove( Time.Delta ) > 0 )
		{
			ThisPawn.Position = helper.Position;
			ThisPawn.Velocity = helper.Velocity;
		}
	}

	//Move with steps
	public void StepMove()
	{
		ProcessMoveHelper( out MoveHelper helper, out float stepSize );

		if ( helper.TryMoveWithStep( Time.Delta, stepSize ) > 0 )
		{
			ThisPawn.Position = helper.Position;
			ThisPawn.Velocity = helper.Velocity;
		}
	}

	//This method is called from both StepMove() and Move()
	private void ProcessMoveHelper( out MoveHelper helper, out float stepSize )
	{
		float groundAngle = ThisPawn.GroundAngle;
		stepSize = ThisPawn.StepSize;

		helper = new( ThisPawn.Position, ThisPawn.Velocity )
		{
			MaxStandableAngle = groundAngle
		};


		helper.Trace = helper.Trace.Size( Hull )
			.Ignore( ThisPawn )
			.WithoutTags( "player" );
	}
	#endregion



	#region Collisions
	public BBox Hull
	{
		get
		{
			float coordinates = _bodyGirth * 0.45f;
			float height = CurrentEyeHeight;

			Vector3 mins = new( -coordinates, -coordinates, 0 );
			Vector3 maxs = new( coordinates, coordinates, height * 1.1f );

			return new BBox( mins, maxs );
		}
	}

	/// <summary>
	/// Traces the bbox and returns the trace result.
	/// LiftFeet will ProcessMoveHelper the start position up by this amount, while keeping the top of the bbox at the same 
	/// position. This is good when tracing down because you won't be tracing through the ceiling above.
	/// </summary>
	public virtual TraceResult TraceBBox( Vector3 start, Vector3 end, Vector3 mins, Vector3 maxs, float liftFeet = 0.0f, float liftHead = 0.0f )
	{
		if ( liftFeet > 0 )
		{
			start += Vector3.Up * liftFeet;
			maxs = maxs.WithZ( maxs.z - liftFeet );
		}

		if ( liftHead > 0 )
		{
			end += Vector3.Up * liftHead;
		}

		var tr = Trace.Ray( start, end )
					.Size( mins, maxs )
					.WithAnyTags( "solid", "playerclip", "passbullets" )
					.Ignore( ThisPawn )
					.Run();

		return tr;
	}

	/// <summary>
	/// This calls TraceBBox with the right sized bbox. You should derive this in your controller if you 
	/// want to use the built in functions
	/// </summary>
	public virtual TraceResult TraceBBox( Vector3 start, Vector3 end, float liftFeet = 0.0f, float liftHead = 0.0f )
	{
		var hull = Hull;
		return TraceBBox( start, end, hull.Mins, hull.Maxs, liftFeet, liftHead );
	}
	#endregion
}
