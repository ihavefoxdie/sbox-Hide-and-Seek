using Sandbox;
namespace HideAndSeek;

public class PawnStats : Component
{
	[Range( 0, 100, 1, true, true )]
	[Property] public int Health { get; set; } = 100;
	[Range( 0, 100, 1, true, true )]
	[Property] public float Stamina { get; set; } = 100;
	[Property] public int MaxStamina { get; set; } = 100;
	[Property] public Status CurrentStatus { get { if ( Health <= 0 ) return Status.Dead; else return Status.Alive; } }
	[Property] public bool IsTired { get; private set; }

	public enum Status
	{
		Dead = 0,
		Alive = 1
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
	}
}
