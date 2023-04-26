﻿using HideAndSeek.Systems.Controllers;
using Sandbox;
using System.Collections.Generic;
using System.Linq;
using HideAndSeek.Systems.Controllers.Movement;

namespace HideAndSeek;

public partial class MainController : EntityComponent<Pawn>, ISingletonComponent
{
	//We need to setup basic stuff like collisions and such
	[Net, Predicted]
	public float CurrentEyeHeight { get; set; } = 64f;
	public float MoveScale { get; set; } = 1f;
	public float CurrentGroundAngle { get; set; }
	public Vector3 Mins { get; set; }
	public Vector3 Maxs { get; set; }
	protected float BodyGirth = 32f;

	public MechanicBaseClass BestMechanic
	{
		get { return AllMechanics.OrderByDescending( mechainc => mechainc.SortOrder ).FirstOrDefault(); }
	}

	public Pawn ThisPawn
	{
		get { return Entity; }
	}


	public IEnumerable<MechanicBaseClass> AllMechanics
	{
		get { return ThisPawn.Components.GetAll<MechanicBaseClass>(); }
	}

	private void ExecuteMechanics()
	{
		foreach ( MechanicBaseClass mechanic in AllMechanics )
		{
			mechanic.Simulate();
		}
	}

	public void Simulate()
	{
		ExecuteMechanics();
	}




	#region Movement related code
	public void Accelerate( Vector3 desiredDirection, float desiredSpeed, float speedLimit, float acceleration )
	{
		ThisPawn.Velocity = PawnMovementPhysics.CalculateAcceleration(ThisPawn.Velocity, desiredDirection, desiredSpeed, speedLimit, acceleration );
	}

	public void ApplyFriction( float stopSpeed, float friction = 1f )
	{
		ThisPawn.Velocity = PawnMovementPhysics.Friction(ThisPawn.Velocity, stopSpeed, friction );
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

		return result *= CurrentGroundAngle.Remap(0, 45, 1, 0.6f);
	}

	public virtual float GetDesiredSpeed()
	{
		return BestMechanic?.DesiredSpeed ?? 180f;
	}

	//Simple Move
	public void Move()
	{
		MoveHelperProcess( out MoveHelper helper, out float stepSize );

		if ( helper.TryMove( Time.Delta ) > 0 )
		{
			ThisPawn.Position = helper.Position;
			ThisPawn.Velocity = helper.Velocity;
		}
	}

	//Move with steps
	public void StepMove()
	{
		MoveHelperProcess( out MoveHelper helper, out float stepSize );

		if ( helper.TryMoveWithStep( Time.Delta, stepSize ) > 0 )
		{
			ThisPawn.Position = helper.Position;
			ThisPawn.Velocity = helper.Velocity;
		}
	}

	//This method is called from both StepMove() and Move()
	private void MoveHelperProcess( out MoveHelper helper, out float stepSize )
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
			float coordinates = BodyGirth * 0.45f;
			float height = CurrentEyeHeight;

			Vector3 mins = new( -coordinates, -coordinates, 0 );
			Vector3 maxs = new( coordinates, coordinates, height * 1.1f );

			return new BBox( mins, maxs );
		}
	}

	/// <summary>
	/// Traces the bbox and returns the trace result.
	/// LiftFeet will MoveHelperProcess the start position up by this amount, while keeping the top of the bbox at the same 
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
