using Sandbox;

namespace HideAndSeek;

public class LogoComponent : Component
{
	[Property] public Texture LogoTexture { get; set; }

	protected override void OnAwake()
	{
		base.OnAwake();
		LogoTexture = Texture.CreateRenderTarget().WithWidth( 1920 ).WithHeight( 1080 ).WithDynamicUsage().Create();
	}

	protected override void OnUpdate()
	{

	}
}
