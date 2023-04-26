using System;
using Sandbox;

namespace HideAndSeek.Systems.Controllers.Movement;

static public class PawnMovementPhysics
{
	public static Vector3 CalculateAcceleration( Vector3 velocity, Vector3 desiredDirection, float desiredSpeed, float speedLimit, float acceleration )
	{
		if ( speedLimit > 0 && desiredSpeed > speedLimit )
			desiredSpeed = speedLimit;

		float currentSpeed = velocity.Dot( desiredDirection );
		float addSpeed = desiredSpeed - currentSpeed;

		if ( addSpeed <= 0 ) return velocity;

		float accelSpeed = acceleration * Time.Delta * desiredSpeed;

		if ( accelSpeed > addSpeed ) accelSpeed = addSpeed;

		return velocity += desiredDirection * accelSpeed;
	}


	public static Vector3 Friction( Vector3 velocity, float stopSpeed, float friction = 1f )
	{
		float speed = velocity.Length;
		if ( speed.AlmostEqual( 0f ) ) return velocity;

		float control = speed;
		if ( speed < stopSpeed ) control = stopSpeed;
		
		float frictionedSpeed = speed - control * friction * Time.Delta;
		if ( frictionedSpeed < 0 ) frictionedSpeed = 0;

		if (frictionedSpeed != speed)
		{
			frictionedSpeed /= speed;
			velocity *= frictionedSpeed;
		}

		return velocity;
	}
}
