using HideAndSeek.Systems.Controllers.Movement;

namespace HideAndSeek.Systems.Controllers;

public class MechanicFactory
{
	MainController _context;

	public MechanicFactory( MainController currentContext )
	{
		_context = currentContext;
	}


	public MechanicBase Gravity()
	{
		return new GravityMechanic( _context, this );
	}
	public MechanicBase Walk()
	{
		return new WalkingMechanic( _context, this );
	}
	public MechanicBase Jump()
	{
		return new JumpMechanic( _context, this );
	}
	public MechanicBase Ground()
	{
		return new OnGroundMechanic( _context, this );
	}
}
