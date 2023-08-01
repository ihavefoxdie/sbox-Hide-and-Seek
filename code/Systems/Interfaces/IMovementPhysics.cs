using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox.Systems.Interfaces
{
	public interface IMovementPhysics
	{
		public Vector3 CalculateAcceleration( Vector3 velocity, Vector3 desiredDirection, float desiredSpeed, float speedLimit, float acceleration );

		public Vector3 Friction( Vector3 velocity, float stopSpeed, float friction = 1f );
	}
}
