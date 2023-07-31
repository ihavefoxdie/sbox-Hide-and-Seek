using Sandbox;
using Sandbox.Systems.Classes;

namespace HideAndSeek.Systems.Controllers.Movement;

public sealed class WalkingMechanic : MechanicBase
{
	public float StopSpeed { get; private set; } = 150f;
	public float Acceleration { get; private set; } = 6f;
	public float GroundFriciton { get; private set; } = 4f;

	public override float? DesiredSpeed { get { return 200f; } }

	public WalkingMechanic( MainController currentContext, MechanicFactory mechanicFactory ) : base( currentContext, mechanicFactory )
	{

	}

	public override void Simulate()
	{
		if ( _context.GroundHandler.GroundEntity.IsValid() )
			Walk();
	}




	/// <summary>
	/// This keeps the Pawn on the ground while moving dawn slopes.
	/// </summary>
	private void StayOnGround()
	{
		Vector3 beginAt = ThisPawn.Position;
		Vector3 finishAt = ThisPawn.Position + Vector3.Down * ThisPawn.StepSize;

		TraceResult trace = CollisionHandler.TraceBBox( ThisPawn, _context.Hull, ThisPawn.Position, beginAt );
		beginAt = trace.EndPosition;

		trace = CollisionHandler.TraceBBox( ThisPawn, _context.Hull, beginAt, finishAt );

		if ( trace.Fraction <= 0 || trace.Fraction >= 1 ||
			trace.StartedSolid || Vector3.GetAngle( Vector3.Up, trace.Normal ) > ThisPawn.GroundAngle )
		{
			return;
		}

		ThisPawn.Position = trace.EndPosition;
	}

	private void Walk()
	{
		Vector3 desiredVelocity = Controller.GetInputVelocity();
		Vector3 desiredDirection = desiredVelocity.Normal;
		float desiredSpeed = desiredVelocity.Length;
		float friction = GroundFriciton * _context.GroundHandler.SurfaceFriciton;

		ThisPawn.Velocity = ThisPawn.Velocity.WithZ( 0 );
		Controller.ApplyFriction( StopSpeed, friction );

		ThisPawn.Velocity = ThisPawn.Velocity.WithZ( 0 );
		Controller.Accelerate( desiredDirection, desiredSpeed, 0, Acceleration );
		ThisPawn.Velocity = ThisPawn.Velocity.WithZ( 0 );

		ThisPawn.Velocity += ThisPawn.BaseVelocity;

		try
		{
			if ( ThisPawn.Velocity.Length < 1f )
			{
				ThisPawn.Velocity = Vector3.Zero;
				return;
			}

			Vector3 destination = (ThisPawn.Position + ThisPawn.Velocity * Time.Delta).WithZ( ThisPawn.Position.z );
			TraceResult trace = CollisionHandler.TraceBBox( ThisPawn, _context.Hull, ThisPawn.Position, destination );

			if ( trace.Fraction == 1 )
			{
				ThisPawn.Position = trace.EndPosition;
				StayOnGround();
				return;
			}

			Controller.StepMove();
		}
		finally
		{
			ThisPawn.Velocity -= ThisPawn.BaseVelocity;
		}

		StayOnGround();
	}



	public override void EnterMechanic()
	{

	}

	public override void SimulateMechanic()
	{
		Simulate();
		CheckSwitchMechanic();
	}

	public override void ExitMechanic()
	{

	}

	public override void CheckSwitchMechanic()
	{

	}

	public override void InitializeSubMechanic()
	{
		throw new System.NotImplementedException();
	}
}
