using Sandbox;
using Sandbox.Citizen;
using Sandbox.VR;

namespace HideAndSeek;

public class BoneComponent : Component
{
	#region Properties
	[Property] public GameObject Foot { get; set; }

	#endregion


	#region Variables

	#endregion

	protected override void OnAwake()
	{
		base.OnAwake();
	}

	protected override void OnUpdate()
	{
		MoveFoot();
	}

	protected override void OnFixedUpdate()
	{
		base.OnFixedUpdate();
	}

	#region Methods
	private void MoveFoot()
	{
		Transform.Rotation = Foot.Transform.Rotation;
		Transform.Position = Foot.Transform.Position;
	}
	#endregion
}
