using Sandbox;
using System.ComponentModel;

namespace HideAndSeek;

public partial class Pawn
{
	//First off, we need eyes.
	[Net, Predicted, Browsable( false )] protected Vector3 LocalEyePosition { get; set; }
	[Net, Predicted, Browsable( false )] protected Rotation LocalEyeRotation { get; set; }

	public Vector3 EyePosition
	{
		get
		{
			return Transform.PointToWorld( LocalEyePosition );
		}
		set
		{
			LocalEyePosition = Transform.PointToLocal( value );
		}
	}
	public Rotation EyeRotation
	{
		get
		{
			return Transform.RotationToWorld( LocalEyeRotation );
		}
		set
		{
			LocalEyeRotation = Transform.RotationToLocal( value );
		}
	}

	public override Ray AimRay
	{
		get { return new( EyePosition, EyeRotation.Forward ); }
	}

	// An example BuildInput method within a player's Pawn class.
	[ClientInput] public Vector3 InputDirection { get; protected set; }
	[ClientInput] public Angles ViewAngles { get; set; }

	public override void BuildInput()
	{
		InputDirection = Input.AnalogMove;

		var look = Input.AnalogLook;

		var viewAngles = ViewAngles;
		viewAngles += look;
		viewAngles.pitch = viewAngles.pitch.Clamp( -80f, 80f );
		ViewAngles = viewAngles.Normal;
	}
}
