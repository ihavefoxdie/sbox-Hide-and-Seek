using Sandbox;
using Sandbox.Utility;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;

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

	private async void Test()
	{
		Package package = await Package.Fetch( "construct", false );
		var hello = package.MountAsync();
		CancellationTokenSource src = new();
		CancellationToken ass = src.Token;
		await hello.WaitAsync( ass );
		ass.Register( () => Log.Info( "FUCK" ) );
	}

	protected override void OnUpdate()
	{
	}
}
