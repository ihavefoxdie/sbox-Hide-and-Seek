using Sandbox;
using Sandbox.Network;
namespace HideAndSeek;

public class PawnModel : Component, Component.INetworkSpawn
{
	[Property] public SkinnedModelRenderer PawmModelRenderer { get; set; }

	protected override void OnAwake()
	{
		base.OnStart();

		PawmModelRenderer = Components.Get<SkinnedModelRenderer>();
	}

	public void OnNetworkSpawn( Connection owner )
	{
		var clothing = new ClothingContainer();
		clothing.Deserialize( owner.GetUserData( "avatar" ) );
		clothing.Apply( PawmModelRenderer );
	}

	protected override void OnUpdate()
	{
		
	}
}
