using Sandbox;

namespace HideAndSeek;

public class CameraMovement : Component
{
	#region Properties
	[Property]
	[Range( 0f, 150f, 0.01f, true, true )]
	public float Distance { get; set; } = 150f;
	[Property] public GameObject Head { get; set; }
	[Property] public GameObject Model { get; private set; }
	[Property] public ModelRenderer PawnRenderer { get { return _pawnRenderer; } }
	[Property] public CameraComponent Camera { get; set; }
	[Property] public bool FloatyCamera { get; set; } = false;
	[Property][Sync] public Angles EyeAngles { get; set; }
	[Property][Sync] public Vector3 EyePosition { get; set; }
	[Property][Sync] public Vector3 EyeLocalPosition { get; set; }
	[Sync] public Vector3 LookingAtPos { get; set; }


	[Property]
	[Range( 0f, 2f, 0.01f, true, true )]
	public float Sensitivity { get; set; } = 0.05f;
	[Property]
	public bool IsFirstPerson
	{
		get
		{
			if ( Distance == 0f ) return true;
			return false;
		}
	}
	#endregion

	#region Variables
	private ModelRenderer _pawnRenderer;
	public Vector3 _cameraPosition;
	#endregion

	protected override void DrawGizmos()
	{
		//if (!Gizmo.IsSelected) return;

		Gizmo.GizmoDraw draw = Gizmo.Draw;
		draw.LineSphere( EyePosition, 10f );
		draw.LineCylinder( EyePosition, EyePosition + Transform.Rotation.Forward * 40f, 5f, 5f, 10 );
	}

	protected override void OnAwake()
	{
		var elements = GameObject.Parent.Children;

		foreach ( var element in elements )
		{
			switch ( element.Name )
			{
				case "Head":
					Head = element;
					continue;
				case "Model":
					Model = element;
					continue;
				default: continue;
			}
		}

		Camera = Components.Get<CameraComponent>();
		_cameraPosition = Head.Transform.Position;
		_pawnRenderer ??= Model.Components.Get<ModelRenderer>();
	}

	protected override void OnUpdate()
	{
		Camera.Enabled = !IsProxy && GameObject.Network.IsOwner;
		if ( IsProxy ) return;

		ProcessRotation();
		ProcessCameraPosition();
	}

	protected override void OnFixedUpdate()
	{
	}



	#region Methods
	private void ProcessRotation()
	{
		Angles eyeAngles = Head.Transform.Rotation.Angles();
		eyeAngles.pitch += Input.MouseDelta.y * Sensitivity;
		eyeAngles.yaw -= Input.MouseDelta.x * Sensitivity;
		eyeAngles.roll = 0f;
		eyeAngles.pitch = eyeAngles.pitch.Clamp( -89f, 89f );
		EyeAngles = eyeAngles;
		Head.Transform.Rotation = Rotation.From( eyeAngles );
	}


	private void ProcessCameraPosition()
	{
		if ( Head == null || PawnRenderer == null ) return;
		EyePosition = Head.Transform.Position;
		EyeLocalPosition = Head.Transform.LocalPosition;

		if ( !IsFirstPerson )
		{
			if ( FloatyCamera )
				_cameraPosition = _cameraPosition.LerpTo( Head.Transform.Position, Time.Delta * 8 );
			else
			{
				_cameraPosition = Head.Transform.Position;
			}

			Vector3 forward = Head.Transform.Rotation.Forward;
			Vector3 left = Head.Transform.Rotation.Left;
			SceneTraceResult cameraTrace = Scene.Trace.Ray( _cameraPosition, _cameraPosition - (forward * Distance) - left*40 )
				.WithoutTags( "pawn", "trigger" )
				.Run();

			if ( cameraTrace.Hit )
			{
				_cameraPosition = cameraTrace.HitPosition + cameraTrace.Normal;
			}
			else
			{
				_cameraPosition = cameraTrace.EndPosition;
			}

			PawnRenderer.RenderType = ModelRenderer.ShadowRenderType.On;
		}
		else
		{
			PawnRenderer.RenderType = ModelRenderer.ShadowRenderType.ShadowsOnly;
		}

		//TODO: fix to avoid pawn dissappearing on for everyone on server
		/*if ( Vector3.DistanceBetween( _cameraPosition, Model.Transform.Position ) < 20 || Vector3.DistanceBetween( _cameraPosition, Head.Transform.Position ) < 20 )
		{
			PawnRenderer.RenderType = ModelRenderer.ShadowRenderType.ShadowsOnly;
		}*/

		Camera.Transform.Position = _cameraPosition;
		Camera.Transform.Rotation = Rotation.From( EyeAngles );
		SceneTraceResult lookingAt = Scene.Trace.Ray( _cameraPosition + Camera.Transform.Rotation.Forward*Distance, _cameraPosition + EyeAngles.Forward * 250 )
				.WithoutTags("maincamera")
				.Run();
		if( lookingAt.Hit )
		{
			LookingAtPos = lookingAt.HitPosition - lookingAt.Normal - Head.Transform.Position;
		}
		else
		{
			LookingAtPos = Head.Transform.Rotation.Forward;
		}


	}
	#endregion
}
