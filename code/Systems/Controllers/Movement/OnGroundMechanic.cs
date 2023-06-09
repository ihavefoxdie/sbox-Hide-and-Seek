﻿using Sandbox;

namespace HideAndSeek.Systems.Controllers.Movement
{
	public partial class OnGroundMechanic : MechanicBase
	{
		public OnGroundMechanic( MainController currentContext, MechanicFactory factory ) : base( currentContext, factory )
		{
			InitializeSubMechanic();
		}

		public override void CheckSwitchMechanic()
		{
			if ( Input.Pressed( "Jump" ) )
			{
				SwitchMechanic( _factory.Jump() );
			}
		}

		public override void EnterMechanic()
		{
			
		}

		public override void ExitMechanic()
		{

		}

		public override void InitializeSubMechanic()
		{
			SetSubMechanic( _factory.Walk() );
		}

		public override void SimulateMechanic()
		{
			CheckSwitchMechanic();
		}
	}
}
