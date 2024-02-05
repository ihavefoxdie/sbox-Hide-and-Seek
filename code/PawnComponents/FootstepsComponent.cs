using Sandbox;
using System;

namespace HideAndSeek;

public class FootstepsComponent : Component
{
	#region Properties
	[Property] public FootComponent LeftFoot { get; set; }
	[Property] public FootComponent RightFoot { get; set; }
	[Property] public PawnComponent Pawn { get; set; }
	private Surface CurrentSurface { get { if ( LeftFoot.CurrentSurface is null ) return RightFoot.CurrentSurface; else return LeftFoot.CurrentSurface; } }
	#endregion

	#region Variables
	private bool _leftStepped;
	private bool _rightStepped;
	#endregion


	protected override void OnUpdate()
	{

	}

	protected override void OnFixedUpdate()
	{
		base.OnFixedUpdate();

		PlaySound( LeftFoot );
		PlaySound( RightFoot );
	}

	protected override void OnAwake()
	{
		base.OnAwake();
		Pawn.JumpAction += JumpSound;
		Pawn.LandedAction += LandedSound;
	}

	#region Methods
	[Broadcast]
	private void JumpSound()
	{
		var sound = "sounds/footsteps/footstep-concrete.sound";
		if ( CurrentSurface != null )
		{
			sound = CurrentSurface.Sounds.FootLaunch;
			if ( sound == "" || sound is null )
			{
				sound = "sounds/footsteps/footstep-concrete.sound";
			}
		}

		Sound.Play( sound, LeftFoot.FootObject.Transform.Position ).Volume = System.Math.Max( 0.5f * Pawn.PawnController.Velocity.Length / Pawn.BaseSpeed, 0.2f );
		Sound.Play( sound, RightFoot.FootObject.Transform.Position ).Volume = System.Math.Max( 0.5f * Pawn.PawnController.Velocity.Length / Pawn.BaseSpeed, 0.2f );
	}

	[Broadcast]
	private void LandedSound()
	{
		var sound = "sounds/footsteps/footstep-concrete.sound";
		if ( CurrentSurface != null )
		{
			sound = CurrentSurface.Sounds.FootLaunch;
			if ( sound == "" || sound is null )
			{
				sound = "sounds/footsteps/footstep-concrete.sound";
			}
		}

		Sound.Play( sound, LeftFoot.FootObject.Transform.Position ).Volume = System.Math.Max(0.5f * Pawn.PawnController.Velocity.Length / Pawn.BaseSpeed, 0.2f);
		Sound.Play( sound, RightFoot.FootObject.Transform.Position ).Volume = System.Math.Max( 0.5f * Pawn.PawnController.Velocity.Length / Pawn.BaseSpeed, 0.2f );

	}


	private void PlaySound( FootComponent foot )
	{
		bool stepped = true;
		switch ( foot.Foot )
		{
			case FootLR.Left:
				stepped = _leftStepped;
				break;
			case FootLR.Right:
				stepped = _rightStepped;
				break;
		}


		if ( !stepped && !foot.IsLifted )
		{
			if ( foot.CurrentSurface is null )
				return;


			var sound = foot.CurrentSurface.Sounds.FootRight;
			if ( foot.Foot == FootLR.Left ) sound = foot.CurrentSurface.Sounds.FootLeft;
			if ( sound == "" || sound is null )
				sound = "sounds/footsteps/footstep-concrete.sound";
			Sound.Play( sound, foot.FootObject.Transform.Position ).Volume = System.Math.Max(1f * Pawn.DesiredVelocity.Length / Pawn.BaseSpeed, 0.2f);


			switch ( foot.Foot )
			{
				case FootLR.Left:
					_leftStepped = true;
					return;
				case FootLR.Right:
					_rightStepped = true;
					return;
			}
		}
		if ( foot.IsLifted )
		{
			switch ( foot.Foot )
			{
				case FootLR.Left:
					_leftStepped = false;
					return;
				case FootLR.Right:
					_rightStepped = false;
					return;
			}
		}
	}
	#endregion
}
