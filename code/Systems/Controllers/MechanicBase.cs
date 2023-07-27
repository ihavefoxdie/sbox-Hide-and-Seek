namespace HideAndSeek.Systems.Controllers;

public abstract class MechanicBase
{
	protected bool _isMainMechanic = false;
	public virtual float? DesiredSpeed { get; set; } = null;
	//public virtual float? EyeHeight { get; set; } = null;

	protected MainController _context;
	protected MechanicFactory _factory;
	private MechanicBase _currentSuperMechanic;
	private MechanicBase _currentSubMechanic;

	public MechanicBase( MainController currentContext, MechanicFactory factory ) : base()
	{
		_context = currentContext;
		_factory = factory;
	}

	public Pawn ThisPawn
	{
		get { return _context.Entity; }
	}
	public MainController Controller
	{
		get { return _context; }
	}

	public abstract void EnterMechanic();
	public abstract void SimulateMechanic();
	public abstract void ExitMechanic();
	public abstract void CheckSwitchMechanic();

	public abstract void InitializeSubMechanic();

	public void SimulateMechanics()
	{
		SimulateMechanic();
		_currentSubMechanic?.SimulateMechanics();
	}

	public virtual void Simulate() { }

	protected void SwitchMechanic( MechanicBase newMechanic )
	{
		ExitMechanic();

		newMechanic.EnterMechanic();
		if ( _isMainMechanic )
			_context.CurrentMechanic = newMechanic;
		else
			_currentSuperMechanic?.SetSubMechanic( newMechanic );
	}
	protected void SetSuperMechanic( MechanicBase superMechanic )
	{
		_currentSuperMechanic = superMechanic;
	}
	protected void SetSubMechanic( MechanicBase subMechanic )
	{
		_currentSubMechanic = subMechanic;
		subMechanic.SetSuperMechanic( this );
	}

	private void MechanicSwitchHandler()
	{

	}
}
