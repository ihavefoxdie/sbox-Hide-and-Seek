using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox.Systems.Classes
{
	public static class CollisionHandler
	{
		/// <summary>
		/// Traces the bbox and returns the trace result.
		/// LiftFeet will ProcessMoveHelper the start position up by this amount, while keeping the top of the bbox at the same 
		/// position. This is good when tracing down because you won't be tracing through the ceiling above.
		/// </summary>
		public static TraceResult TraceBBox(Entity Pawn, Vector3 start, Vector3 end, Vector3 mins, Vector3 maxs, float liftFeet = 0.0f, float liftHead = 0.0f )
		{
			if ( liftFeet > 0 )
			{
				start += Vector3.Up * liftFeet;
				maxs = maxs.WithZ( maxs.z - liftFeet );
			}

			if ( liftHead > 0 )
			{
				end += Vector3.Up * liftHead;
			}

			var tr = Trace.Ray( start, end )
						.Size( mins, maxs )
						.WithAnyTags( "solid", "playerclip", "passbullets" )
						.Ignore( Pawn )
						.Run();

			return tr;
		}

		/// <summary>
		/// This calls TraceBBox with the right sized bbox. You should derive this in your controller if you 
		/// want to use the built in functions
		/// </summary>
		public static TraceResult TraceBBox( Entity pawn, BBox Hull, Vector3 start, Vector3 end, float liftFeet = 0.0f, float liftHead = 0.0f )
		{
			var hull = Hull;
			return TraceBBox( pawn, start, end, hull.Mins, hull.Maxs, liftFeet, liftHead );
		}
	}
}
