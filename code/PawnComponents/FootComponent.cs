using Sandbox;

namespace HideAndSeek;

public enum FootLR
{
	Left = 0, Right = 1
}

public class FootComponent : Component
{
	#region Properties
	[Property] public bool IsLifted { get; private set; }
	[Property] public GameObject FootObject {  get; set; }
	[Property] public FootLR Foot { get; set; }
	[Property] public Surface CurrentSurface { get; private set; }
	#endregion

	protected override void OnFixedUpdate()
	{
		base.OnFixedUpdate();

		CheckLifted();
	}

	#region Methods
	private void CheckLifted()
	{
		SceneTraceResult collision = Scene.Trace.Ray( FootObject.Transform.Position, FootObject.Transform.Position + Vector3.Down * 1f )
			.WithoutTags( "pawn", "trigger" )
			.Radius( 4f )
			.Run();

		CurrentSurface = collision.Surface;
		if (collision.Hit && IsLifted )
		{
			IsLifted = false;
			return;
		}
		else if (collision.Hit && !IsLifted )
		{
			return;
		}
		IsLifted = true;
	}
	#endregion
}
