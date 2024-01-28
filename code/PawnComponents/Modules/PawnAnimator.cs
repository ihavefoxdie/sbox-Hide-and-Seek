using Sandbox;
using Sandbox.Citizen;

namespace HideAndSeek.PawnComponents.Modules
{
	public static partial class PawnAnimator
	{
		private static Vector3 RotationTilt(PawnMovement pawn)
		{
			if ( pawn.PawnController.Velocity.Dot( Vector3.Left * pawn.Model.Transform.Rotation ) > 20 && pawn.Rotated )
			{
				return Vector3.Right * 5000 * pawn.Head.Transform.Rotation.Angles();
			}
			else if ( pawn.PawnController.Velocity.Dot( Vector3.Left * pawn.Model.Transform.Rotation ) < -20 && pawn.Rotated )
			{
				return Vector3.Left * 5000 * pawn.Head.Transform.Rotation.Angles();
			}
			return 1;
		}

		public static void AnimationUpdate( PawnMovement pawn )
		{
			if ( pawn == null ) { return; }
			if ( pawn.AnimationHelper is null )
			{ return; }

			pawn.AnimationHelper.WithWishVelocity( pawn.DesiredVelocity + Vector3.Zero.LerpTo(RotationTilt(pawn), Time.Delta * 12));
			pawn.AnimationHelper.WithVelocity( pawn.PawnController.Velocity);
			pawn.AnimationHelper.WithLook( pawn.Head.Transform.Rotation.Forward, 1f, 0.8f, 1f );
			pawn.AnimationHelper.DuckLevel = pawn.IsDucking ? 1 : 0;
			pawn.AnimationHelper.AimAngle = pawn.Head.Transform.Rotation;
			pawn.AnimationHelper.HoldType = CitizenAnimationHelper.HoldTypes.None;
			pawn.AnimationHelper.IsGrounded = pawn.PawnController.IsOnGround;
			pawn.AnimationHelper.MoveStyle = CitizenAnimationHelper.MoveStyles.Auto;
			//pawn.AnimationHelper.FootShuffle = 0.000001f;
		}

	}
}
