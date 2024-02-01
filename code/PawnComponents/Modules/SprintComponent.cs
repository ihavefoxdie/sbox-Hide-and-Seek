using HideAndSeek;
using Sandbox;
using Sandbox.Services;

public class SprintComponent : Component
{
	#region Properties
	[Property] public PawnComponent Pawn { get { return _pawn; } }
	[Property] public float RegenCooldown { get; set; } = 1;
	#endregion

	#region Variables
	private PawnComponent _pawn;
	private float _sinceSprint;
	#endregion

	protected override void OnAwake()
	{
		base.OnAwake();
		_sinceSprint = Time.Now;
		_pawn = Components.Get<PawnComponent>();
	}

	protected override void OnUpdate()
	{
		SprintCheck();
	}

	protected override void OnFixedUpdate()
	{
		base.OnFixedUpdate();

	}

	#region Methods
	private void SprintCheck()
	{
		if ( !Pawn.IsSprinting || Pawn.PawnController.Velocity.IsNearlyZero() || Pawn.IsDucking || !Pawn.PawnController.IsOnGround )
		{
			if ( Pawn.Stats.Stamina < Pawn.Stats.MaxStamina  && Time.Now - _sinceSprint > RegenCooldown )
			{
				Pawn.Stats.Stamina += 0.15f * (Pawn.Stats.MaxStamina / 100);
			}
		}
		else if ( !Pawn.IsDucking && Pawn.IsSprinting && !Pawn.PawnController.Velocity.IsNearlyZero() && Pawn.PawnController.IsOnGround)
		{
			if ( Pawn.Stats.Stamina > 0)
			{
				Pawn.Stats.Stamina -= 0.25f;
				_sinceSprint = Time.Now;
			}
		}
	}

	private void SprintAcceleration()
	{
		if ( Pawn.IsSprinting && Pawn.Stats.Stamina > 0 )
			Pawn.DesiredVelocity *= Pawn.SpritDelta;
	}
	#endregion
}
