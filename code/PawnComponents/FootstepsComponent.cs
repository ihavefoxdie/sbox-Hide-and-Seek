using Sandbox;

namespace HideAndSeek;

public class FootstepsComponent : Component
{
	#region Properties
	[Property] public FootComponent LeftFoot { get; set; }
	[Property] public FootComponent RightFoot { get; set; }
	#endregion

	#region Variables
	private bool _leftStepped;
	private bool _rightStepped;
	#endregion

	protected override void OnUpdate()
	{
		PlaySound( LeftFoot );
		PlaySound( RightFoot );
	}

	protected override void OnAwake()
	{
		base.OnAwake();
	}

	#region Methods
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
				foot.CurrentSurface.PlayCollisionSound( foot.FootObject.Transform.Position );
			else
				Sound.Play( sound, foot.FootObject.Transform.Position ).Volume = 0.5f;


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
		if( foot.IsLifted )
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
