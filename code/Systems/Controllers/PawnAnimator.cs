using Sandbox;
using System;

namespace HideAndSeek.Systems.Controllers;

public partial class PawnAnimator : EntityComponent<Pawn>, ISingletonComponent
{


	private Vector3 RotationTilt()
	{
		Vector3 tilt = new( 0 );
		if ( Entity.Velocity.Dot( Vector3.Left * Entity.Rotation ) > 20 && Entity.Rotated )
		{
			return Vector3.Right * 8000 * Entity.ViewAngles.ToRotation();
		}
		else if ( Entity.Velocity.Dot( Vector3.Left * Entity.Rotation ) < -20 && Entity.Rotated )
		{
			return Vector3.Left * 8000 * Entity.ViewAngles.ToRotation();
		}
		return 0;
	}

	public virtual void Simulate( IClient client )
	{

		DebugOverlay.ScreenText( Entity.Velocity.Dot( Vector3.Left * Entity.Rotation ).ToString(), 9 );
		DebugOverlay.ScreenText( Entity.Rotated.ToString(), 8 );


		CitizenAnimationHelper animHelper = new( Entity );
		animHelper.WithWishVelocity( Entity.Controller.GetInputVelocity() + Vector3.Lerp( Vector3.Zero, RotationTilt(), Time.Delta ) );
		animHelper.WithVelocity( Entity.Velocity );
		animHelper.WithLookAt( Entity.EyePosition + Entity.EyeRotation.Forward * 100.0f, 1.0f, 0.8f, 1f );
		
		animHelper.AimAngle = Entity.EyeRotation;
		animHelper.HoldType = CitizenAnimationHelper.HoldTypes.None;
		animHelper.IsGrounded = Entity.Controller.GroundHandler.GroundEntity.IsValid();
		animHelper.MoveStyle = CitizenAnimationHelper.MoveStyles.Auto;
		animHelper.FootShuffle = 0f;
	}
}
