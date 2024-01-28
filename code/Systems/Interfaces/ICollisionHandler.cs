using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox.Systems.Interfaces
{
	public interface ICollisionHandler
	{
		public TraceResult TraceBBox( Vector3 start, Vector3 end, Vector3 mins, Vector3 maxs, Entity toIgnore, float liftFeet = 0.0f, float liftHead = 0.0f );
	}
}
