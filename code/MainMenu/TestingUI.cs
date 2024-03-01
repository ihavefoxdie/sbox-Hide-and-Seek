using Sandbox;
using Sandbox.UI;

namespace HideAndSeek;

public class TestingUI : Panel
{
	public Texture Texture { get; set; }

	protected override void OnParametersSet()
	{
		this.Style.BackgroundImage = Texture ?? Texture.Invalid;
	}

	protected override int BuildHash() => Texture is null ? 0 : Texture.GetHashCode();
}
