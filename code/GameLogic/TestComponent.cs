using Sandbox;
using System.Collections.Generic;
using System.Linq;

namespace HideAndSeek;

public class TestComponent : Component
{
	private List<GameObject> Doors { get; set; }

	protected override void OnStart()
	{
		base.OnStart();
		Doors = new();

		Doors = Scene.GetAllObjects( true ).Where( x => x.Name == "ent_door" ).ToList();
		Doors.ForEach( x => x.Destroy() );
		
	}

	protected override void OnUpdate()
	{

	}
}
