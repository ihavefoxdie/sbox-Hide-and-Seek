using HideAndSeek;
using HideAndSeek.Systems.Controllers;
using Sandbox;

namespace Sandbox.Systems.Controllers.Mechanics;

public class CrouchMechanic : MechanicBase
{
	public override float DesiredSpeed
	{
		get
		{
			return 120f;
		}
	}

	public override float? EyeHeight => 40f;
	public CrouchMechanic( MainController currentContext, MechanicFactory factory ) : base( currentContext, factory )
	{
	}

	public override void CheckSwitchMechanic()
	{
		throw new System.NotImplementedException();
	}

	public override void EnterMechanic()
	{
		throw new System.NotImplementedException();
	}

	public override void ExitMechanic()
	{
		throw new System.NotImplementedException();
	}

	public override void InitializeSubMechanic()
	{
		throw new System.NotImplementedException();
	}

	public override void SimulateMechanic()
	{
		throw new System.NotImplementedException();
	}
}
