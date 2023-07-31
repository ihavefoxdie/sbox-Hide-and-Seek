using HideAndSeek.Systems.Controllers;
using HideAndSeek.Systems.Controllers.Movement;
using Sandbox;
using Sandbox.Systems.Classes;
using System.Collections.Generic;

namespace HideAndSeek;

public partial class MainController : EntityComponent<Pawn>, ISingletonComponent
{
	//We need to setup basic stuff like collisions and such
	[Net, Predicted]
	public float CurrentEyeHeight { get; set; } = 64f;
	public float MoveScale { get; set; } = 1f;
	protected float _bodyGirth = 32f;
	public float BodyGirth
	{
		get
		{
			return _bodyGirth;
		}
	}
	public float MaxGroundVelocity { get; private set; } = 150f;
	public float StepSize { get; private set; } = 12f;

	public MechanicBase CurrentMechanic;
	public MechanicFactory Factory;
	public List<MechanicBase> Mechanics { get; private set; }
	public GroundHandler GroundHandler { get; private set; }



	public MainController() : base()
	{
		GroundHandler = new( this );
		Factory = new MechanicFactory( this );
		Mechanics = new List<MechanicBase>();
		CurrentMechanic = Factory.Gravity();
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
		GroundHandler.CategorizePosition( GroundHandler.GroundEntity != null );
		ExecuteMechanics();
	}



	#region Ground Related Logic
	
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

		return result *= GroundHandler.CurrentGroundAngle.Remap( 0, 45, 1, 0.6f );
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
	#endregion
}
