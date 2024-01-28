using Sandbox;
using Sandbox.Citizen;

public sealed class PawnMovement : Component
{
	#region General Properties
	[Property] public float AirSpeed { get; set; } = 50f;
	[Property] public float AirFriction { get; } = 0.25f;
	[Property] public float WalkingSpeed { get; set; } = 100f;
	[Property] public float SpritDelta { get; set; } = 100f;
	[Property] public float CrouchDelta { get; set; } = -50f;
	[Property] public float Friction { get; set; } = 1f;
	[Property] public CharacterController CharacterController { get { return _characterController; } }
	[Property] public GameObject Head { get { return _head; } }
	[Property] public GameObject Model { get { return _model; } }
	#endregion

	#region References
	private GameObject _head;
	private GameObject _model;
	#endregion

	#region Member Variables
	public Vector3 DesiredVelocity { get; set; }
	[Property] public bool IsCrouching { get; set; }
	[Property] public bool IsRunning { get; set; }
	private CharacterController _characterController;
	private CitizenAnimationHelper _animationHelper;
	#endregion



	protected override void OnAwake()
	{
		var elements = GameObject.Children;
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

		_characterController = Components.Get<CharacterController>();
		_animationHelper = Components.Get<CitizenAnimationHelper>();
		base.OnAwake();
	}

	protected override void OnUpdate()
	{
		IsCrouching = Input.Down( "Duck" );
		IsRunning = Input.Down( "Run" );
	}

	protected override void OnFixedUpdate()
	{
		CalculateDesiredVelocity();
		Move();
	}

	#region Methods
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

		if ( IsCrouching )
			DesiredVelocity *= WalkingSpeed + CrouchDelta;
		else if ( IsRunning )
			DesiredVelocity *= WalkingSpeed + SpritDelta;
		else
			DesiredVelocity *= WalkingSpeed;
	}

	private void Move()
	{
		Vector3 gravity = Scene.PhysicsWorld.Gravity;

		if ( CharacterController.IsOnGround )
		{
			CharacterController.Velocity = CharacterController.Velocity.WithZ( 0 );
			CharacterController.Accelerate( DesiredVelocity );
			CharacterController.ApplyFriction( Friction );
		}
		else
		{
			CharacterController.Velocity += gravity * Time.Delta * 0.5f;
			CharacterController.Accelerate( DesiredVelocity.ClampLength( AirSpeed ) );
			CharacterController.ApplyFriction( AirFriction );
		}

		CharacterController.Move();

		if ( !CharacterController.IsOnGround )
		{
			CharacterController.Velocity += gravity * Time.Delta * 0.5f;
		}
		else
		{
			CharacterController.Velocity = CharacterController.Velocity.WithZ( 0 );
		}
	}

	private void RotateModel()
	{

	}

	// Redundant function the older project used to rely on. It stays here for a while.
	private Vector3 CalculateAcceleration( Vector3 velocity, Vector3 desiredDirection, float desiredSpeed, float speedLimit, float acceleration )
	{
		if ( speedLimit > 0 && desiredSpeed > speedLimit )
			desiredSpeed = speedLimit;

		float currentSpeed = velocity.Dot( desiredDirection );
		float addSpeed = desiredSpeed - currentSpeed;

		if ( addSpeed <= 0 ) return velocity;

		float accelSpeed = acceleration * Time.Delta * desiredSpeed;

		if ( accelSpeed > addSpeed ) accelSpeed = addSpeed;

		return velocity += desiredDirection * accelSpeed;
	}
	#endregion
}
