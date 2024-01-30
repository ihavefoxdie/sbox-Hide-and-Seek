using Sandbox;

namespace HideAndSeek;

public class CameraMovement : Component
{
	#region Properties
	[Property] public PawnMovement Pawn { get; private set; }
	[Property]
	[Range( 0f, 150f, 0.01f, true, true )]
	public float Distance { get; set; } = 150f;
	[Property] public GameObject Head { get; private set; }
	[Property] public GameObject Model { get; private set; }
	[Property] public ModelRenderer PawnRenderer { get { return _pawnRenderer; } }
	[Property] public CameraComponent Camera { get { return _camera; } }
	[Property] public bool FloatyCamera { get; set; } = true;
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
	private CameraComponent _camera;
	private ModelRenderer _pawnRenderer;
	private Vector3 _cameraPosition;
	#endregion

	protected override void OnAwake()
	{
		base.OnAwake();
		Pawn = Components.GetInParent<PawnMovement>();
		Head = Pawn.Head;
		Model = Pawn.Model;
		_cameraPosition = Head.Transform.Position;
		_pawnRenderer = Model.Components.Get<ModelRenderer>();
		_camera = Components.Get<CameraComponent>();
		base.OnAwake();
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		ProcessRotation();
	}

	protected override void OnFixedUpdate()
	{
		base.OnFixedUpdate();
		ProcessCameraPosition();
	}



	#region Methods
	private void ProcessRotation()
	{
		Angles eyeAngles = Head.Transform.Rotation.Angles();
		eyeAngles.pitch += Input.MouseDelta.y * Sensitivity;
		eyeAngles.yaw -= Input.MouseDelta.x * Sensitivity;
		eyeAngles.roll = 0f;
		eyeAngles.pitch = eyeAngles.pitch.Clamp( -89f, 89f );
		Head.Transform.Rotation = Rotation.From( eyeAngles );
	}

	private void ProcessCameraPosition()
	{
		if ( _camera == null ) return;

		if ( !IsFirstPerson )
		{
			if ( FloatyCamera )
				_cameraPosition = _cameraPosition.LerpTo( Head.Transform.Position, Time.Delta * 8 );
			else
			{
				_cameraPosition = Head.Transform.Position;
				//Distance *= 10;
			}
			Vector3 forward = Head.Transform.Rotation.Forward;
			SceneTraceResult cameraTrace = Scene.Trace.Ray( _cameraPosition, _cameraPosition - (forward * Distance) )
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

		if ( Vector3.DistanceBetween( _cameraPosition, Model.Transform.Position ) < 20 || Vector3.DistanceBetween( _cameraPosition, Head.Transform.Position ) < 20 )
		{
			PawnRenderer.RenderType = ModelRenderer.ShadowRenderType.ShadowsOnly;
		}
		Camera.Transform.Position = _cameraPosition;
		Camera.Transform.Rotation = Head.Transform.Rotation;
	}
	#endregion
}
