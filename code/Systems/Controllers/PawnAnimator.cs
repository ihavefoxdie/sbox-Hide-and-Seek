using Sandbox;

namespace HideAndSeek.Systems.Controllers;

public partial class PawnAnimator: EntityComponent<Pawn>, ISingletonComponent
{
	public void Update(Pawn pawn)
	{
		CitizenAnimationHelper animHelper = new( pawn )
		{
			AimAngle = pawn.EyeRotation
		};
		animHelper.WithVelocity( pawn.Velocity );
		animHelper.WithLookAt( pawn.EyePosition );
		animHelper.HoldType = CitizenAnimationHelper.HoldTypes.None;
		animHelper.IsGrounded = true;
		animHelper.MoveStyle = CitizenAnimationHelper.MoveStyles.Auto;
		animHelper.FootShuffle = 0f;
	}
}
