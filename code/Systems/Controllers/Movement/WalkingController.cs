using Sandbox;

namespace HideAndSeek.Systems.Controllers.Movement;

public partial class WalkingController : MechanicBaseClass
{
	public float Speed { get; private set; } = 300f;
	public float StopSpeed { get; private set; } = 150f;
	public float WalkSpeed { get; private set; } = 150f;
	public float MaxGroundVelocity { get; private set; } = 150f;
	public float Acceleration { get; private set; } = 6f;
	public float DuckedAcceleration { get; private set; } = 5f;
	public float StepSize { get; private set; } = 12f;
	public float GoundAngle { get; private set; } = 45f;
	public float SurfaceFriciton { get; set; } = 1f;
	public float GroundFriciton { get; private set; } = 4f;

	public override float DesiredSpeed { get { return 200f; } }

	public override void Simulate()
	{

		if ( ThisPawn.GroundEntity == null )
		{
			ThisPawn.Velocity = Vector3.Down * 100;
			Move();
			CheckGoundEntity( TraceBBox( ThisPawn.Position, ThisPawn.Position - Vector3.Up * 2, 4.0f ) );
		}
		else
			Walk();

	}

	public void SetGroundEntity( Entity entity )
	{
		ThisPawn.GroundEntity = entity;

	}


	/// <summary>
	/// This keeps the Pawn on the ground while moving dawn slopes.
	/// </summary>
	private void StayOnGround()
	{
		Vector3 beginAt = ThisPawn.Position;
		Vector3 finishAt = ThisPawn.Position + Vector3.Down * ThisPawn.StepSize;

		TraceResult trace = TraceBBox( ThisPawn.Position, beginAt );
		beginAt = trace.EndPosition;

		trace = TraceBBox( beginAt, finishAt );

		if ( trace.Fraction <= 0 || trace.Fraction >= 1 ||
			trace.StartedSolid || Vector3.GetAngle( Vector3.Up, trace.Normal ) > ThisPawn.GroundAngle )
		{
			return;
		}

		ThisPawn.Position = trace.EndPosition;
	}

	private void Walk()
	{
		Vector3 desiredVelocity = GetInputVelocity();
		Vector3 desiredDirection = desiredVelocity.Normal;
		float desiredSpeed = desiredVelocity.Length;
		float friction = GroundFriciton * SurfaceFriciton;

		ThisPawn.Velocity = ThisPawn.Velocity.WithZ( 0 );
		ApplyFriction( StopSpeed, friction );

		ThisPawn.Velocity = ThisPawn.Velocity.WithZ( 0 );
		Accelerate( desiredDirection, desiredSpeed, 0, Acceleration );
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
			TraceResult trace = TraceBBox(ThisPawn.Position, destination);

			if(trace.Fraction == 1)
			{
				ThisPawn.Position = trace.EndPosition;
				StayOnGround();
				return;
			}

			StepMove();
		}
		finally
		{
			ThisPawn.Velocity -= ThisPawn.BaseVelocity;
		}

		StayOnGround();
	}

	public void ClearGorundEntity()
	{
		//if ( GroundEntity == null ) return;

		
	}

	//This method determines the ground entity. It also can tell us if there is no ground at all
	private void CheckGoundEntity( TraceResult trace )
	{
		/*if ( trace.Surface.Friction * 1.25f > 1 )
			return;*/

		SetGroundEntity( trace.Entity );
	}
}
