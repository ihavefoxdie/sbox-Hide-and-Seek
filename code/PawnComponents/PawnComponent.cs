﻿using HideAndSeek.PawnComponents.Modules;
using Sandbox;
using Sandbox.Citizen;
using System;

namespace HideAndSeek;

public class PawnComponent : Component
{
	#region General Properties
	[Property, Category( "Measurements" )] public float AirSpeed { get; set; } = 50f;
	[Property, Category( "Measurements" )] public float AirFriction { get; } = 0.25f;
	[Property, Category( "Measurements" )] public float BaseSpeed { get; set; } = 200;
	[Property, Category( "Measurements" )] public float WalkingDelta { get; set; } = -100f;
	[Property, Category( "Measurements" )] public float SpritDelta { get; set; } = 100f;
	[Property, Category( "Measurements" )] public float DuckDelta { get; set; } = -125f;
	[Property, Category( "Measurements" )] public int DuckHeightDelta { get { return -(int)(_initPawnHeight / 2f); } }
	[Property, Category( "Measurements" )] public float InitHeight { get { return _initPawnHeight; } }
	[Property, Category( "Measurements" )] public float JumpForce { get; set; } = 300f;
	[Property, Category( "Measurements" )] public float Friction { get; set; } = 4f;

	[Property, Category( "Components" )] public CharacterController PawnController { get; set; }
	[Property, Category( "Components" )] public CitizenAnimationHelper AnimationHelper { get; set; }
	[Property, Category( "Components" )] public PawnStats Stats { get; set; }
	[Property, Category( "GameObjects" )] public GameObject Head { get; set; }
	[Property, Category( "GameObjects" )] public GameObject Model { get; set; }
	[Property, Category( "GameObjects" )] public GameObject CameraObject { get; set; }
	[Property, Category( "Components" )] public CameraMovement Camera { get; set; }

	[Property][Sync] public float CurrentHeight { get { return PawnController.Height; } }
	[Property][Sync] public bool Rotated { get; private set; }
	[Property][Sync] public bool IsDucking { get; private set; }
	[Property][Sync] public bool IsSprinting { get; private set; }
	[Property][Sync] public bool IsWalking { get; private set; }
	[Property] public Vector3 DesiredVelocity { get; set; }
	public Vector3 VelocityForTurn { get; set; }
	#endregion

	#region Actions
	public Action JumpAction { get; set; }
	public Action LandedAction { get; set; }
	public Action CrouchAction { get; set; }
	public Action SprintAction { get; set; }
	#endregion

	#region Member Variables
	private Rotation _lastRotation;
	private bool _jumped;
	private float _initPawnHeight;
	#endregion

	//TODO: fix often thrown exception on failed proper component initialization.
	protected override void OnStart()
	{
		var elements = GameObject.Children;
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
				case "Camera":
					CameraObject = element;
					continue;
				default: continue;
			}
		}

		_lastRotation = Head.Transform.Rotation;
		PawnController = Components.Get<CharacterController>();
		AnimationHelper = Components.GetInChildren<CitizenAnimationHelper>();
		CameraObject.Components.Create<CameraMovement>();
		Camera = CameraObject.Components.Get<CameraMovement>();
		_initPawnHeight = PawnController.Height;
		JumpAction += Jump;
	}


	protected override void OnUpdate()
	{
		if ( Model == null )
		{
			Log.Info( "THIS DAMN model IS NULL!!!" );
		}
		if ( Head == null )
		{
			Log.Info( "THIS DAMN head IS NULL!!!" );
		}
		if ( Camera == null )
		{
			Log.Info( "THIS DAMN camera IS NULL!!!" );
			Camera = Components.GetInChildren<CameraMovement>( true );
		}


		RotateModel();
		PawnAnimator.AnimationUpdate( this );
		VelocityForTurn = PawnController.Velocity;
		if ( IsProxy ) return;
		if ( Head == null ) return;

		Head.Transform.LocalPosition = Head.Transform.LocalPosition.WithZ( (PawnController.Height - 10).Clamp( 40, InitHeight ) );
		IsSprinting = Input.Down( "Run" );
		IsWalking = Input.Down( "Walk" );
		if ( Input.Pressed( "Jump" ) ) JumpAction?.Invoke();
	}

	protected override void OnFixedUpdate()
	{
		if ( IsProxy ) return;
		DuckCheck();
		RotationCheck();
		CalculateDesiredVelocity();
		Move();
	}

	#region Methods
	private void DuckCheck()
	{
		float delta = _initPawnHeight - PawnController.Height;

		if ( Input.Down( "Duck" ) )
		{
			PawnController.Height = PawnController.Height.LerpTo( _initPawnHeight + DuckHeightDelta, 8 * Time.Delta );
			IsDucking = true;
		}
		else if ( !Input.Down( "Duck" ) && IsDucking )
		{
			SceneTraceResult collision = Scene.Trace.Ray( Head.Transform.Position, Head.Transform.Position + Vector3.Up * PawnController.Height * 0.4f )
				.WithoutTags( "pawn", "trigger" )
				.Radius( 5f )
				.Run();

			if ( !collision.Hit && PawnController.IsOnGround )
			{
				//what a fucking hack!
				PawnController.Height = PawnController.Height.LerpTo( _initPawnHeight, 8 * Time.Delta );
				if ( PawnController.Height.CeilToInt() < _initPawnHeight.CeilToInt() )
					IsDucking = true;
				else
				{
					PawnController.Height = 71;
					IsDucking = false;
				}
			}
		}
		delta -= _initPawnHeight - PawnController.Height;
		if ( IsDucking && !PawnController.IsOnGround )
		{
			PawnController.Transform.Position += new Vector3( 0, 0, delta * -1 );
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
			DesiredVelocity *= BaseSpeed + DuckDelta;
		else if ( IsSprinting && Stats.Stamina > 0 )
			DesiredVelocity *= BaseSpeed + SpritDelta;
		else if ( IsWalking )
			DesiredVelocity *= BaseSpeed + WalkingDelta;
		else
			DesiredVelocity *= BaseSpeed;
	}

	private void Move()
	{
		Vector3 gravity = Scene.PhysicsWorld.Gravity;

		if ( PawnController.IsOnGround )
		{
			if ( _jumped )
			{
				_jumped = false;
				LandedAction?.Invoke();
			}
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
		_jumped = true;
		PawnController.Punch( Vector3.Up * JumpForce );
		AnimationHelper?.TriggerJump();
	}

	private void RotateModel()
	{
		if ( Model == null || PawnController == null || Camera == null ) return;
		Vector3 lookAt = Camera.LookingAtPos - Model.Transform.LocalPosition;


		if ( Math.Abs( Rotation.From(lookAt.EulerAngles.WithPitch(0).WithRoll(0)).Angle() - Model.Transform.Rotation.Angle() ) > 1 )
		{
			if ( PawnController.Velocity.IsNearZeroLength )
			{
				VelocityForTurn = 5f;
			}
			Model.Transform.Rotation = Rotation.Lerp( Model.Transform.Rotation, lookAt.EulerAngles.WithRoll( 0 ).WithPitch( 0 ), Time.Delta * 20f );
		}
	}
	#endregion
}
