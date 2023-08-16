using System.Collections.Generic;
using Sandbox;
using Sandbox.Systems.Classes;
using Sandbox.Systems.Interfaces;
using HideAndSeek.Systems.Controllers;

namespace HideAndSeek;

public partial class MainController : EntityComponent<Pawn>, ISingletonComponent
{
	//We need to setup basic stuff like collisions and such
	[Net, Predicted]
	public float CurrentEyeHeight { get; set; } = 72f;
	public float EyeHeight => MainMechanic.EyeHeight ?? 72f;
	

	public float MoveScale { get; set; } = 1f;
	public float MaxGroundVelocity { get; private set; } = 150f;
	public float StepSize { get; private set; } = 12f;
	protected float _bodyGirth = 32f;
	public float BodyGirth
	{
		get { return _bodyGirth; }
	}
	public Pawn Pawn
	{
		get { return Entity; }
	}

	public MechanicBase MainMechanic;
	public MechanicFactory Factory;
	public List<MechanicBase> Mechanics { get; private set; }
	public GroundHandler GroundHandler { get; private set; }
	public ICollisionHandler Collisions { get; private set; }
	public IMovementPhysics MovementPhysics { get; private set; }

	public MainController() : base()
	{
		MovementPhysics = new PawnMovementPhysics();
		GroundHandler = new( this );
		Collisions = new CollisionHandler();
		Factory = new MechanicFactory( this );
		Mechanics = new List<MechanicBase>();
		MainMechanic = Factory.Gravity();
	}



	private void ExecuteMechanics()
	{
		if ( MainMechanic != null )
		{
			MainMechanic.SimulateMechanics();
		}
	}

	protected void SimulateEyes()
	{
		DebugOverlay.ScreenText( CurrentEyeHeight.ToString(), 12 );
		var target = EyeHeight;
		DebugOverlay.ScreenText( target.ToString(), 13 );
		// Magic number :sad:
		var trace = Collisions.TraceBBox( Pawn.Position, Pawn.Position, Hull.Mins, Hull.Maxs, Pawn, 0, 10f);
		if ( trace.Hit && target > CurrentEyeHeight )
		{
			// We hit something, that means we can't increase our eye height because something's in the way.
			int a = 0;
		}
		else
		{
			CurrentEyeHeight = CurrentEyeHeight.LerpTo( target, Time.Delta * 10f );
		}

		Pawn.EyeRotation = Pawn.ViewAngles.ToRotation();
		Pawn.LocalEyePosition = Vector3.Up * CurrentEyeHeight;
	}

	public void Simulate( IClient client )
	{
		GroundHandler.CategorizePosition();
		ExecuteMechanics();
		SimulateEyes();
	}

	public void FrameSimulate( IClient client )
	{
		SimulateEyes();
	}


	#region Movement related code
	public void Accelerate( Vector3 desiredDirection, float desiredSpeed, float speedLimit, float acceleration )
	{
		Pawn.Velocity = MovementPhysics.CalculateAcceleration( Pawn.Velocity, desiredDirection, desiredSpeed, speedLimit, acceleration );
	}

	public void ApplyFriction( float stopSpeed, float friction = 1f )
	{
		Pawn.Velocity = MovementPhysics.Friction( Pawn.Velocity, stopSpeed, friction );
	}

	public Vector3 GetInputVelocity( bool zeroPitch, float desiredSpeed = 180f )
	{
		Vector3 result = new( Pawn.InputDirection.x, Pawn.InputDirection.y, 0 );
		result *= MoveScale;

		Vector3 inMovement = result.Length.Clamp( 0f, 1f );

		result *= Pawn.ViewAngles.WithPitch( 0f ).ToRotation();

		if ( zeroPitch ) result.z = 0f;

		result = result.Normal * inMovement;
		result *= desiredSpeed;

		return result *= GroundHandler.CurrentGroundAngle.Remap( 0, 45, 1, 0.6f );
	}


	/*public float GetDesiredSpeed()
	{
		return MainMechanic?.DesiredSpeed ?? 180f;
	}*/

	//Simple Move
	public void Move()
	{
		ProcessMoveHelper( out MoveHelper helper, out _ );

		if ( helper.TryMove( Time.Delta ) > 0 )
		{
			Pawn.Position = helper.Position;
			Pawn.Velocity = helper.Velocity;
		}
	}

	//Move with steps
	public void StepMove()
	{
		ProcessMoveHelper( out MoveHelper helper, out float stepSize );

		if ( helper.TryMoveWithStep( Time.Delta, stepSize ) > 0 )
		{
			Pawn.Position = helper.Position;
			Pawn.Velocity = helper.Velocity;
		}
	}

	//This method is called from both StepMove() and Move()
	private void ProcessMoveHelper( out MoveHelper helper, out float stepSize )
	{
		float groundAngle = Pawn.GroundAngle;
		stepSize = Pawn.StepSize;

		helper = new( Pawn.Position, Pawn.Velocity )
		{
			MaxStandableAngle = groundAngle
		};


		helper.Trace = helper.Trace.Size( Hull )
			.Ignore( Pawn )
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
	#endregion
}
