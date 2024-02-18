using Sandbox;
using Sandbox.Citizen;

namespace HideAndSeek;

public class TeamEquipmentComponent : Component
{
	private PawnComponent Pawn;
	private CitizenAnimationHelper AnimationHelper;

	protected override void OnStart()
	{
		Pawn = Components.Get<PawnComponent>();
	}

	protected override void OnFixedUpdate()
	{
		if ( Pawn is null ) return;

		if ( AnimationHelper == null )
		{
			var animHelper = Components.GetInChildren<CitizenAnimationHelper>( true );
			if ( animHelper is not null )
			{
				AnimationHelper = animHelper;
			}
		}
		else
		{
			AnimationHelper.HoldType = CitizenAnimationHelper.HoldTypes.Swing;
		}
	}
}
