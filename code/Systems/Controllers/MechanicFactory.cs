using HideAndSeek.Systems.Controllers.Movement;
using System.Linq;

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
		for ( int i = 0; i < _context.Mechanics.Count; i++ )
		{
			if ( _context.Mechanics[i] is GravityMechanic )
				return _context.Mechanics[i];
		}
		_context.Mechanics.Add( new GravityMechanic( _context, this ) );
		return Gravity();
	}
	public MechanicBase Walk()
	{
		for ( int i = 0; i < _context.Mechanics.Count; i++ )
		{
			if ( _context.Mechanics[i] is WalkingMechanic )
				return _context.Mechanics[i];
		}
		_context.Mechanics.Add( new WalkingMechanic( _context, this ) );
		return Walk();
	}
	public MechanicBase Jump()
	{
		for ( int i = 0; i < _context.Mechanics.Count; i++ )
		{
			if ( _context.Mechanics[i] is JumpMechanic )
				return _context.Mechanics[i];
		}
		_context.Mechanics.Add( new JumpMechanic( _context, this ) );
		return Jump();
	}
	public MechanicBase Ground()
	{
		for ( int i = 0; i < _context.Mechanics.Count; i++ )
		{
			if ( _context.Mechanics[i] is OnGroundMechanic )
				return _context.Mechanics[i];
		}
		_context.Mechanics.Add( new OnGroundMechanic( _context, this ) );
		return Ground();
	}
}
