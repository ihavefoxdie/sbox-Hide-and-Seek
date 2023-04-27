using Sandbox;

namespace HideAndSeek.Systems.Controllers.Movement;

public partial class AirMove: MechanicBaseClass
{
	public float Gravity { get { return 800f; } }
	public float AirControl { get { return 30f; } }
	public float AirAcceleration { get { return 35f; } }

	public override void Simulate()
	{
		ThisPawn.Velocity -= new Vector3( 0, 0, Gravity * 0.5f ) * Time.Delta;
		ThisPawn.Velocity -= new Vector3( 0, 0, ThisPawn.BaseVelocity.z) * Time.Delta;
		ThisPawn.BaseVelocity = ThisPawn.BaseVelocity.WithZ( 0 );

		bool groundedAtStart = ThisPawn.GroundEntity.IsValid();
		if ( groundedAtStart ) return;

		Vector3 desiredVelocity = Controller.GetInputVelocity( true );
		Vector3 desiredDirection = desiredVelocity.Normal;
		float desiredSpeed = desiredVelocity.Length;

		Controller.Accelerate(desiredDirection, desiredSpeed, AirControl, AirAcceleration);
		ThisPawn.Velocity += ThisPawn.BaseVelocity;
		Controller.Move();
		ThisPawn.Velocity -= ThisPawn.BaseVelocity;
		ThisPawn.Velocity -= new Vector3( 0, 0, Gravity * 0.5f ) * Time.Delta;
	}
}
