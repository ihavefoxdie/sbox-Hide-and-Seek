using HideAndSeek.PawnComponents.Modules;
using Sandbox;
using Sandbox.Citizen;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HideAndSeek;

public class PawnComponent : Component
{
	#region General Properties
	[Property] public float AirSpeed { get; set; } = 50f;
	[Property] public float AirFriction { get; } = 0.25f;
	[Property] public float WalkingSpeed { get; set; } = 100f;
	[Property] public float SpritDelta { get; set; } = 100f;
	[Property] public float CrouchDelta { get; set; } = -50f;
	[Property] public float JumpForce { get; set; } = 300f;
	[Property] public float Friction { get; set; } = 3f;
	[Property] public CharacterController PawnController { get { return _characterController; } }
	[Property] public CitizenAnimationHelper AnimationHelper { get { return _animationHelper; } }
	[Property] public PawnStats Stats { get { return _stats; } }
	[Property] public GameObject Head { get { return _head; } }
	[Property] public GameObject Model { get { return _model; } }
	[Property] public bool Rotated { get; private set; }
	[Property] public bool IsDucking { get; private set; }
	[Property] public bool IsRunning { get; private set; }
	public Vector3 DesiredVelocity { get; private set; }
	#endregion

	#region References
	private GameObject _head;
	private GameObject _model;
	private CharacterController _characterController;
	private CitizenAnimationHelper _animationHelper;
	private PawnStats _stats;
	#endregion

	#region Member Variables
	private Rotation _lastRotation;
	private List<SpawnPoint> _spawnPoints;
	#endregion



	protected override void OnAwake()
	{
		base.OnAwake();

		_spawnPoints = Scene.GetAllComponents<SpawnPoint>().ToList<SpawnPoint>();
		var elements = this.GameObject.Children;
		foreach ( var element in elements )
		{
			switch ( element.Name )
			{
				case "Head":
					_head = element;
					continue;
				case "Model":
					_model = element;
					continue;
				default: continue;
			}
		}
		
		_stats = Components.Get<PawnStats>();
		_characterController = Components.Get<CharacterController>();
		_animationHelper = Components.GetInChildren<CitizenAnimationHelper>();
		_lastRotation = Head.Transform.Rotation;
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		PawnAnimator.AnimationUpdate( this );
		IsRunning = Input.Down( "Run" );
		if ( Input.Pressed( "Jump" ) )
		{
			Jump();
		}
	}

	protected override void OnFixedUpdate()
	{
		base.OnUpdate();

		DuckCheck();
		RotationCheck();
		CalculateDesiredVelocity();
		Move();
		RotateModel();
	}

	#region Methods
	[ConCmd("respawn")]
	private void Spawn()
	{
		if ( _spawnPoints is null || _spawnPoints.Count < 1 )
			return;

		GameObject.Transform.World = Random.Shared.FromList( _spawnPoints, default ).GameObject.Transform.World;
	}

	private void DuckCheck()
	{
		if ( Input.Down( "Duck" ) && !IsDucking )
		{
			PawnController.Height /= 1.5f;
			IsDucking = true;
			Head.Transform.LocalPosition = Head.Transform.LocalPosition.LerpTo( Vector3.Zero.WithZ( 40 ), Time.Delta * 100 );

		}
		if ( !Input.Down( "Duck" ) && IsDucking )
		{
			SceneTraceResult collision = Scene.Trace.Ray( Head.Transform.Position, Head.Transform.Position + Vector3.Up * PawnController.Height * 0.4f )
				.WithoutTags( "pawn", "trigger" )
				.Radius( 20f )
				.Run();

			if ( !collision.Hit )
			{
				PawnController.Height *= 1.5f;
				IsDucking = false;
				Head.Transform.LocalPosition = Head.Transform.LocalPosition.LerpTo( Vector3.Zero.WithZ( 60 ), Time.Delta * 100 );
			}
		}
	}

	private void RotationCheck()
	{
		if ( Rotation.Difference( _lastRotation, Head.Transform.Rotation ).Angle() > 1f )
		{
			Rotated = true;
			_lastRotation = Head.Transform.Rotation;
		}
		else
		{
			Rotated = false;
		}
	}

	private void CalculateDesiredVelocity()
	{
		DesiredVelocity = 0;

		Rotation rotation = Head.Transform.Rotation;
		if ( Input.Down( "Forward" ) )
			DesiredVelocity += rotation.Forward;
		if ( Input.Down( "Left" ) )
			DesiredVelocity += rotation.Left;
		if ( Input.Down( "Right" ) )
			DesiredVelocity += rotation.Right;
		if ( Input.Down( "Backward" ) )
			DesiredVelocity += rotation.Backward;

		DesiredVelocity = DesiredVelocity.WithZ( 0 );

		if ( !DesiredVelocity.IsNearZeroLength )
			DesiredVelocity = DesiredVelocity.Normal;

		if ( IsDucking )
			DesiredVelocity *= WalkingSpeed + CrouchDelta;
		else if ( IsRunning )
			DesiredVelocity *= WalkingSpeed + SpritDelta;
		else
			DesiredVelocity *= WalkingSpeed;
	}

	private void Move()
	{
		Vector3 gravity = Scene.PhysicsWorld.Gravity;

		if ( PawnController.IsOnGround )
		{
			PawnController.Velocity = PawnController.Velocity.WithZ( 0 );
			PawnController.Accelerate( DesiredVelocity );
			PawnController.ApplyFriction( Friction );
		}
		else
		{
			PawnController.Velocity += gravity * Time.Delta * 0.5f;
			PawnController.Accelerate( DesiredVelocity.ClampLength( AirSpeed ) );
			PawnController.ApplyFriction( AirFriction );
		}

		PawnController.Move();

		if ( !PawnController.IsOnGround )
		{
			PawnController.Velocity += gravity * Time.Delta * 0.5f;
		}
		else
		{
			PawnController.Velocity = PawnController.Velocity.WithZ( 0 );
		}
	}

	private void Jump()
	{
		if ( !PawnController.IsOnGround )
		{
			return;
		}
		PawnController.Punch( Vector3.Up * JumpForce );
		_animationHelper?.TriggerJump();
	}

	private void RotateModel()
	{
		if ( Model is null )
		{ return; }

		Angles targetAngle = new( 0, Head.Transform.Rotation.Yaw(), 0f );
		float rotateDifference = Model.Transform.Rotation.Distance( targetAngle );

		if ( PawnController.Velocity.Length > 10f )
		{
			Model.Transform.Rotation = Rotation.Lerp( Model.Transform.Rotation, targetAngle, Time.Delta * 2f );
		}
		else if ( rotateDifference > 175f )
		{
			Model.Transform.Rotation = Rotation.Lerp( Model.Transform.Rotation, targetAngle, Time.Delta * 1f );
		}
	}
	#endregion
}
