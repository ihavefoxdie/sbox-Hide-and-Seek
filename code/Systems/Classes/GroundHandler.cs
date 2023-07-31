using HideAndSeek;

namespace Sandbox.Systems.Classes
{
	public class GroundHandler
	{
		private MainController _context { get; set; }
		public float CurrentGroundAngle { get; set; }
		public Vector3 GroundNormal { get; set; }
		public float MaxGroundVelocity { get; private set; } = 150f;
		public float StepSize { get; private set; } = 12f;
		public float GroundAngle { get; private set; } = 45f;
		public float SurfaceFriciton { get; set; } = 1f;
		public Entity GroundEntity { get; set; }

		public BBox Hull
		{
			get
			{
				float coordinates = _context.BodyGirth * 0.45f;
				float height = _context.CurrentEyeHeight;

				Vector3 mins = new( -coordinates, -coordinates, 0 );
				Vector3 maxs = new( coordinates, coordinates, height * 1.1f );

				return new BBox( mins, maxs );
			}
		}

		public GroundHandler( MainController context )
		{
			_context = context;
		}

		public void SetGroundEntity( Entity entity )
		{
			GroundEntity = entity;
			if ( GroundEntity != null )
			{
				_context.ThisPawn.Velocity = _context.ThisPawn.Velocity.WithZ( 0 );
				_context.ThisPawn.BaseVelocity = GroundEntity.Velocity;
			}
		}

		public void CategorizePosition( bool stayOnGround )
		{
			SurfaceFriciton = 1f;

			Vector3 point = _context.ThisPawn.Position - Vector3.Up * 2;
			Vector3 bumpOrigin = _context.ThisPawn.Position;
			bool rapidlyMovingUp = _context.ThisPawn.Velocity.z > MaxGroundVelocity;
			bool moveToEndPosition = false;

			//if we leave the ground, there's no need to process anything else
			if ( rapidlyMovingUp )
			{
				ClearGorundEntity();
				return;
			}

			if ( GroundEntity != null || stayOnGround )
			{
				moveToEndPosition = true;
				point.z -= StepSize;
			}

			TraceResult trace = CollisionHandler.TraceBBox( _context.ThisPawn, Hull, bumpOrigin, point, 4.0f );

			float angle = Vector3.GetAngle( Vector3.Up, trace.Normal );
			CurrentGroundAngle = angle;

			if ( trace.Entity == null || angle > GroundAngle )
			{
				ClearGorundEntity();
				moveToEndPosition = false;

				if ( _context.ThisPawn.Velocity.z > 0 )
					SurfaceFriciton = 0.25f;
			}
			else
			{
				CheckGoundEntity( trace );
			}

			if ( moveToEndPosition && !trace.StartedSolid && trace.Fraction > 0f && trace.Fraction < 1f )
			{
				_context.ThisPawn.Position = trace.EndPosition;
			}
		}

		public void ClearGorundEntity()
		{
			if ( GroundEntity == null ) return;

			GroundEntity = null;
			SurfaceFriciton = 1.0f;
		}

		//This method determines the ground entity. It also can tell us if there is no ground at all
		private void CheckGoundEntity( TraceResult trace )
		{
			GroundNormal = trace.Normal;

			SurfaceFriciton = trace.Surface.Friction * 1.25f;
			if ( SurfaceFriciton > 1f ) SurfaceFriciton = 1f;

			SetGroundEntity( trace.Entity );
		}
	}
}
