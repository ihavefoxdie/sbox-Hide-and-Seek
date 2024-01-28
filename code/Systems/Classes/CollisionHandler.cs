using Sandbox.Systems.Interfaces;

namespace Sandbox.Systems.Classes
{
	public class CollisionHandler: ICollisionHandler
	{
		/// <summary>
		/// Traces the bbox and returns the trace result.
		/// LiftFeet will ProcessMoveHelper the start position up by this amount, while keeping the top of the bbox at the same 
		/// position. This is good when tracing down because you won't be tracing through the ceiling above.
		/// </summary>
		public TraceResult TraceBBox( Vector3 start, Vector3 end, Vector3 mins, Vector3 maxs, Entity toIgnore, float liftFeet = 0.0f, float liftHead = 0.0f )
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
						.Ignore( toIgnore )
						.Run();

			return tr;
		}
	}
}
