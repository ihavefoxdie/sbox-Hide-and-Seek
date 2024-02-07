using Sandbox;
namespace HideAndSeek;

public class PawnModel : Component
{
	[Property][Sync] public SkinnedModelRenderer PawmModelRenderer { get; set; }

	protected override void OnAwake()
	{
		base.OnAwake();

		//PawmModelRenderer = Components.Get<SkinnedModelRenderer>();
		var clothing = ClothingContainer.CreateFromLocalUser();
		clothing.Apply( PawmModelRenderer );
	}

	protected override void OnUpdate()
	{

	}
}
