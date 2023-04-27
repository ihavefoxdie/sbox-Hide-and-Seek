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
	public float GroundAngle { get; private set; } = 45f;
	public float SurfaceFriciton { get; set; } = 1f;
	public float GroundFriciton { get; private set; } = 4f;

	public override float? DesiredSpeed { get { return 200f; } }

	public override void Simulate()
	{
		if ( ThisPawn.GroundEntity != null )
			Walk();

		CategorizePosition( ThisPawn.GroundEntity != null );
	}

	public void SetGroundEntity( Entity entity )
	{
		ThisPawn.GroundEntity = entity;
		if ( ThisPawn.GroundEntity != null )
		{
			ThisPawn.Velocity = ThisPawn.Velocity.WithZ( 0 );
			ThisPawn.BaseVelocity = ThisPawn.GroundEntity.Velocity;
		}
	}


	/// <summary>
	/// This keeps the Pawn on the ground while moving dawn slopes.
	/// </summary>
	private void StayOnGround()
	{
		Vector3 beginAt = ThisPawn.Position;
		Vector3 finishAt = ThisPawn.Position + Vector3.Down * ThisPawn.StepSize;

		TraceResult trace = Controller.TraceBBox( ThisPawn.Position, beginAt );
		beginAt = trace.EndPosition;

		trace = Controller.TraceBBox( beginAt, finishAt );

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
		float friction = GroundFriciton * SurfaceFriciton;

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
			TraceResult trace = Controller.TraceBBox( ThisPawn.Position, destination );

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

		if ( ThisPawn.GroundEntity != null || stayOnGround )
		{
			moveToEndPosition = true;
			point.z -= StepSize;
		}

		TraceResult trace = Controller.TraceBBox( bumpOrigin, point, 4.0f );

		float angle = Vector3.GetAngle( Vector3.Up, trace.Normal );
		Controller.CurrentGroundAngle = angle;

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
		if ( ThisPawn.GroundEntity == null ) return;

		ThisPawn.GroundEntity = null;
		SurfaceFriciton = 1.0f;
	}

	//This method determines the ground entity. It also can tell us if there is no ground at all
	private void CheckGoundEntity( TraceResult trace )
	{
		Controller.GroundNormal = trace.Normal;

		SurfaceFriciton = trace.Surface.Friction * 1.25f;
		if ( SurfaceFriciton > 1f ) SurfaceFriciton = 1f;

		SetGroundEntity( trace.Entity );
	}
}
