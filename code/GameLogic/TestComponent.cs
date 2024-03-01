using Sandbox;
using Sandbox.Modals;
using Sandbox.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Sandbox.GameLogic.Modules;
using System.Threading;
using System.Xml.Serialization;
using Sandbox.UI;
using System.Security.Cryptography;

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
		CancellationTokenSource src = new CancellationTokenSource();
		CancellationToken ass = src.Token;
		await hello.WaitAsync( ass );
		ass.Register( () => Log.Info("FUCK") );
		
	}

	protected override void OnUpdate()
	{
	}
}
