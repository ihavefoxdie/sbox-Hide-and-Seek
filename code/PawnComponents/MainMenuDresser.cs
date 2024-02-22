using Sandbox;

public sealed class MainMenuDresser : Component
{
	[Property] private SkinnedModelRenderer PawnModelRenderer { get; set; }

	protected override void OnAwake()
	{
		var clothing = ClothingContainer.CreateFromLocalUser();
		clothing.Apply( PawnModelRenderer );
	}

	protected override void OnUpdate()
	{

	}
}
