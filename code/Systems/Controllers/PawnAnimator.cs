using Sandbox;
using System.Numerics;

namespace HideAndSeek.Systems.Controllers;

public partial class PawnAnimator: EntityComponent<Pawn>, ISingletonComponent
{
	public virtual void Simulate(IClient client)
	{
		CitizenAnimationHelper animHelper = new( Entity );
		animHelper.WithWishVelocity( Entity.Controller.GetInputVelocity() );
		animHelper.WithVelocity( Entity.Velocity );
		animHelper.WithLookAt( Entity.EyePosition + Entity.EyeRotation.Forward * 100.0f, 1.0f, 1.0f, 0.5f );
		animHelper.AimAngle = Entity.EyeRotation;
		animHelper.HoldType = CitizenAnimationHelper.HoldTypes.None;
		animHelper.IsGrounded = Entity.GroundEntity != null;
		//animHelper.MoveStyle = CitizenAnimationHelper.MoveStyles.Auto;
		animHelper.FootShuffle = 0f;
	}
}
