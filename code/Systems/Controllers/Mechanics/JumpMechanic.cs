using Sandbox;

namespace HideAndSeek.Systems.Controllers.Mechanics;

public partial class JumpMechanic : MechanicBase
{
	private float Gravity => 700f;

	public JumpMechanic( MainController currentContext, MechanicFactory mechanicFactory ) : base( currentContext, mechanicFactory )
	{

	}

	public override void Simulate()
	{
		float flGroundFactor = 1.0f;
		float flMul = 300f;
		float startZ = ThisPawn.Velocity.z;

		ThisPawn.Velocity = ThisPawn.Velocity.WithZ( startZ + flMul * flGroundFactor );
		ThisPawn.Velocity -= new Vector3( 0, 0, Gravity * 0.5f ) * Time.Delta;
	}

	public override void EnterMechanic()
	{
		_context.GroundHandler.ClearGorundEntity();
	}

	public override void SimulateMechanic()
	{
		if ( _context.GroundHandler.GroundEntity.IsValid() )
			Simulate();
		CheckSwitchMechanic();
	}

	public override void ExitMechanic()
	{
		
	}

	public override void CheckSwitchMechanic()
	{
		if ( _context.GroundHandler.GroundEntity.IsValid() )
		{
			SwitchMechanic( _factory.Ground() );
		}
	}

	public override void InitializeSubMechanic()
	{

	}
}
