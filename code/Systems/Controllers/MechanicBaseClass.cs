using Sandbox;

namespace HideAndSeek.Systems.Controllers;

public partial class MechanicBaseClass : EntityComponent<Pawn>
{
	public virtual int SortOrder { get; set; } = 0;
	public virtual float? DesiredSpeed { get; set; } = null;
	//public virtual float? EyeHeight { get; set; } = null;

	public Pawn ThisPawn
	{
		get { return Entity; }
	}
	public MainController Controller
	{
		get { return Entity.Controller; }
	}

	public virtual void Simulate()
	{

	}
}
