using Sandbox;
using Sandbox.Network;
namespace HideAndSeek;

public class PawnModel : Component, Component.INetworkSpawn
{
	[Property] public SkinnedModelRenderer PawmModelRenderer { get; set; }

	protected override void OnStart()
	{
		base.OnStart();

		PawmModelRenderer = Components.Get<SkinnedModelRenderer>();
	}

	public void OnNetworkSpawn( Connection owner )
	{
		var clothing = ClothingContainer.CreateFromLocalUser();
		clothing.Apply( PawmModelRenderer );
	}

	protected override void OnUpdate()
	{
		
	}
}
