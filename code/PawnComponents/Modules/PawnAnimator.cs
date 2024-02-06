using Sandbox;
using Sandbox.Citizen;

namespace HideAndSeek.PawnComponents.Modules;

public static class PawnAnimator
{
	private static Vector3 RotationTilt( PawnComponent pawn )
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

	public static void AnimationUpdate( PawnComponent pawn )
	{
		if ( pawn == null ) { return; }
		if ( pawn.AnimationHelper is null )
		{ return; }

		pawn.AnimationHelper.WithWishVelocity( pawn.PawnController.Velocity + Vector3.Zero.LerpTo( RotationTilt( pawn ), Time.Delta * 2 ) );
		pawn.AnimationHelper.WithVelocity( pawn.PawnController.Velocity );
		Vector3 lookTowards = pawn.Head.Transform.Rotation.Forward;
		pawn.AnimationHelper.WithLook( lookTowards, 0.1f, 0.0f, 0.1f );
		//pawn.AnimationHelper.DuckLevel = 1 - ((pawn.Head.Transform.LocalPosition.z) * 2/(pawn.InitHeight) - 1);
		pawn.AnimationHelper.DuckLevel = pawn.IsDucking ? 1 - ((pawn.Head.Transform.LocalPosition.z) * 2 / (pawn.InitHeight) - 1) : 0;
		pawn.AnimationHelper.AimAngle = pawn.Head.Transform.Rotation;
		pawn.AnimationHelper.HoldType = CitizenAnimationHelper.HoldTypes.None;
		pawn.AnimationHelper.IsGrounded = pawn.PawnController.IsOnGround;
		pawn.AnimationHelper.MoveStyle = CitizenAnimationHelper.MoveStyles.Auto;
	}

}
