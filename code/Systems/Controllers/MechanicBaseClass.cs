using Sandbox;

namespace HideAndSeek.Systems.Controllers;

public partial class MechanicBaseClass : MainController
{
	public virtual int SortOrder { get; set; } = 0;
	public virtual float DesiredSpeed { get; set; }
	public new virtual void Simulate()
	{

	}
}
