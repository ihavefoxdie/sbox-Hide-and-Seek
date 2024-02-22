using Sandbox;
using Sandbox.Citizen;
using Sandbox.ModelEditor.Nodes;
using System.Linq;

namespace HideAndSeek;

public class TeamEquipmentComponent : Component
{
	#region Property
	[Property] public float SwingCooldown { get; set; } = 0.5f;
	[Property] public float SwingRange { get; set; } = 50f;
	[Property] public CameraMovement CameraMovement { get; set; }
	private SyncComponent SyncComponent { get; set; }
	#endregion

	#region Variable
	private PawnComponent _pawn;
	private CitizenAnimationHelper _animationHelper;
	private TimeSince _lastSwing;
	#endregion

	protected override void OnStart()
	{
		_pawn = Components.Get<PawnComponent>();
		CameraMovement = Components.GetInChildren<CameraMovement>();
		_lastSwing = 0;
		SyncComponent = Scene.GetAllComponents<SyncComponent>().Last();
	}
	//TODO: fix animation playing on every pawn upon input from ANY player (who is not the object owner).
	protected override void OnUpdate()
	{
		base.OnUpdate();

		if ( IsProxy ) return;
		if ( Input.Pressed( "attack1" ) && _lastSwing >= SwingCooldown )
			Swing();
	}

	protected override void OnFixedUpdate()
	{
		if ( _pawn is null ) return;

		if ( _animationHelper == null )
		{
			var animHelper = Components.GetInChildren<CitizenAnimationHelper>( true );
			if ( animHelper is not null )
			{
				_animationHelper = animHelper;
			}
		}
		else
		{
			_animationHelper.HoldType = CitizenAnimationHelper.HoldTypes.Swing;
		}
	}

	#region Methods
	[Broadcast]
	private void PlaySwing()
	{
		if ( _animationHelper == null ) return;
		_animationHelper.Target.Set( "b_attack", true );
	}

	private void Swing()
	{
		if ( _animationHelper == null ) return;
		PlaySwing();
		var trace = Scene.Trace.FromTo( CameraMovement.EyePosition, CameraMovement.EyePosition + CameraMovement.EyeAngles.Forward * SwingRange )
			.Size( 20f )
			.IgnoreGameObjectHierarchy(GameObject)
			.Run();

		if(trace.Hit)
		{
			if ( trace.GameObject != null )
			{
				var parent = trace.GameObject.Parent;
				Log.Info( parent );
				bool check = parent.Tags.Has( "hiders" );
				Log.Info( check );
				if(check)
				{
					SyncComponent.CurrentGame.Caught( _pawn.GameObject.Id, parent.Id );
				}
			}
		}

		_lastSwing = 0;
	}
	#endregion
}
