using System;
using Godot;
using static ISIS.Static;

#if GODOT_Real_IS_DOUBLE
using Real = System.Double;
#else
using Real = System.Single;
#endif

namespace ISIS.Minds.SteeringBehaviors {
	/// <summary>
	/// Pure steering behavior functions that return the steer vector as a fraction of LinearVLimit.
	/// </summary>
	public static partial class SteeringBehaviors {
		public static Vector3 SeekPosition(
			Vector3 currentPosition,
			Vector3 targetPosition
		) => /* currentVelocity - */ (targetPosition - currentPosition).Normalized();
		public static Vector3 FleePosition(
			Vector3 currentPosition,
			Vector3 targetPosition
		) => /* -(currentVelocity */ -SeekPosition(currentPosition, targetPosition);

		public static Vector3 InterceptObject(
			Vector3 currentPosition,
			Real travelSpeed,
			RigidBody objectRigidBody
		) => InterceptObject(currentPosition, travelSpeed, objectRigidBody.GlobalTransform.origin, objectRigidBody.LinearVelocity);

		public static Vector3 InterceptObject(
			Vector3 currentPosition,
			Real travelSpeed,
			Vector3 objectPosition,
			Vector3 objectVelocity
		) => SeekPosition(
			currentPosition,
			FindInterceptionPosition(currentPosition, travelSpeed, objectPosition, objectVelocity));

		public static Vector3 FindInterceptionPosition(
			Vector3 currentPosition,
			Real travelSpeed,
			RigidBody targetRigidBody
		) => FindInterceptionPosition(currentPosition, travelSpeed, targetRigidBody.GlobalTransform.origin, targetRigidBody.LinearVelocity);

		public static Vector3 FindInterceptionPosition(
			Vector3 currentPosition,
			Real travelSpeed,
			Vector3 targetPosition,
			Vector3 targetVelocity
		) {
			// FIXME improve
			// possible accept predition time as a parameter

			// calculate predection time
			var relativePosition = targetPosition - currentPosition;
			var distanceToTarget = relativePosition.Length();
			var timeToTargetPosition = distanceToTarget / travelSpeed;
			// time to target intercept position

			return targetPosition + (timeToTargetPosition * targetVelocity);
		}

		public static Vector3 AvoidObstacle(
			Vector3 obstacleCenter,
			Real obstacleRadius,
			Transform currentTransform) {
			// linearInputModifier.z *= -.5f; // brake

			// add the opposite of the collision point vector
			//toPosition -= (info.point * collisionAvoidanceVectorMultiplier);

			var forward = currentTransform.Orthonormalized().basis.z;

			var toCenterOfObstacle = obstacleCenter - currentTransform.origin;
			/* 
			var toCenterAlongForward = forward * toCenterOfObstacle.Dot(forward);
			var fromCenterToForwardAxis = toCenterAlongForward.Normalized() - toCenterOfObstacle.Normalized(); 
			*/
			var fromCenterToForwardAxis = toCenterOfObstacle.Normalized() - forward;
			return SeekPosition(currentTransform.origin, fromCenterToForwardAxis.Normalized() * (obstacleRadius * 1.25f));

			//toRotation = toPosition -= (_latestRaycastHitInfo.point);
			//toPosition = -fromForwardToCenter;
		}

		/// <summary>
		/// 	Obstacle avoidance behavior  on Sebastian Lague's solution.
		/// 	https://github.com/SebLague/Boids
		/// 	Will cast rays that are in an increasing angular difference from the castRoot direction
		///     till a non obstracted direction is found and moves in that direction.
		/// </summary>
		/// <param name="castRoot">
		/// 	Direction from which to sweep outwards from to find a non obstucted one.
		/// </param>
		/// <param name="castForObstruction">
		/// 	A lambda that'll raycast and return wether it hit something or not.
		/// 	bool castForObstruction(Vector3 from, Vector3 to).
		/// 	Handle the collisionMask and exclusion in the lambda yourself.
		/// </param>
		public static Vector3 AvoidObstacleSebLague(
			Vector3 castRoot,
			Real raycastDistance,
			Func<Vector3, Vector3, bool> castForObstruction,
			Transform currentTransform,
			Node node = null
		) {
			var currentPosition = currentTransform.origin;

			// since we'll be testing from the castRoot vector outwards (not the forward vector)
			// we can't use the object's transform
			var transformer = new Transform(BasisFacingDirection(castRoot), currentPosition);

			Vector3[] rayDirections = BoidHelper.directions;
			for (int i = 0; i < BoidHelper.DirectionCount; i++) {
				var dir = transformer.TransformDirection(rayDirections[i]);
#if DEBUG 
				// node?.DebugDraw().Call("draw_line_3d", currentPosition, flockAverageCenter, new Color(1, 1, 0));
				node?.DebugDraw().Call("draw_ray_3d", currentPosition, dir, raycastDistance, Colors.Pink);
#endif
				if (!castForObstruction(currentPosition, dir * raycastDistance)) {
					return dir;
				}
			}
			return Vector3.Zero;
		}

		// FIXME: improve
		// lifted from Craig Reynolds' OpenSteer
		public static Vector3 FollowPath(
			Vector3 currentPosition,
			Vector3 currentVelocity,
			Real requredProximity,
			Func<Vector3, Vector3> closestPointOnPathToPoint,
			Func<Vector3, float> distanceOfPointAlongPath,
			Func<float, Vector3> pathDistanceToPoint,
			int direction,
			Real predictionTime
		) {
			// predict our future position
			var futurePosition = currentPosition + (predictionTime * currentVelocity);

			// measure distance along path of our current and predicted positions
			var currentPathDistance = distanceOfPointAlongPath(currentPosition);
			var futurePathDistance = distanceOfPointAlongPath(futurePosition);

			// are we facing in the correction direction?
			var rightway = (direction > 0) ?
				(currentPathDistance < futurePathDistance) :
				(currentPathDistance > futurePathDistance);

			// find the point on the path nearest the predicted future position
			var onPath = closestPointOnPathToPoint(futurePosition);

			var distanceFromPathSq = futurePosition.DistanceSquaredTo(onPath);

			// no steering is required if (a) our future position is inside
			// the path tube and (b) we are travelling in the correct direction
			if (rightway && (distanceFromPathSq < requredProximity * requredProximity)) {
				// all is well, return zero steering
				return Vector3.Forward;
			} else {
				// otherwise we need to steer towards a target point obtained
				// by adding some offset to our current path position

				// our goal will be offset from our path distance by this amount
				// FIXME: pathDistanceOffset will be zero if speed is zero
				var pathDistanceOffset = (direction * predictionTime * currentVelocity.Length()) + 1;

				float targetPathDistance = currentPathDistance + pathDistanceOffset;
				var target = pathDistanceToPoint(targetPathDistance);

				// return steering to seek target on path
				return SeekPosition(currentPosition, target);
			}
		}

		// TODO: test this
		public static Vector3 StayOnPath(
			Vector3 currentPosition,
			Vector3 currentVelocity,
			Real requredProximity,
			Func<Vector3, Vector3> closestPointOnPathToPoint,
			Real predictionTime
		) {
			// predict our future position
			var futurePosition = currentPosition + (predictionTime * currentVelocity);

			var onPath = closestPointOnPathToPoint(futurePosition);

			var distanceFromPathSq = onPath.DistanceSquaredTo(futurePosition);

			if (distanceFromPathSq > requredProximity * requredProximity) {
				// our predicted future position was in the path,
				// return zero steering.
				return Vector3.Zero;
			} else {
				// our predicted future position was outside the path, need to
				// steer towards it.  Use onPath projection of futurePosition
				// as seek target
				return SeekPosition(currentPosition, onPath);
			}
		}

		/// <summary>
		/// Assumes the craft is in the flock.
		/// </summary>
		public static Vector3 Cohesion(Boids.Flock flock, Vector3 currentPosition) {
			var flockMemberCount = flock.Count;
			if (flockMemberCount > 1) {
				// subtract current position since flock includes current craft
				// and we didn'exclude it when it was orginally summed
				var exculidngCenterSum = flock.CenterSum - currentPosition;
				// subtract from count by one to exclude current craft
				var flockAverageCenter = exculidngCenterSum / (flockMemberCount - 1);

				var seekVector = SeekPosition(currentPosition, flockAverageCenter);
#if DEBUG 
				flock.DebugDraw().Call("draw_line_3d", currentPosition, flockAverageCenter, new Color(1, 1, 0));
				// flock.DebugDraw().Call("draw_ray_3d", currentPosition, seekVector, 200, new Color(1, 1, 0));
#endif
				return seekVector;
			}
			return Vector3.Zero;
		}

		/// <summary>
		/// Assumes the craft is in the flock.
		/// </summary>
		public static Vector3 Alignment(Boids.Flock flock, Vector3 craftForwardDirection) {
			var flockMemberCount = flock.Count;
			if (flockMemberCount > 1) {
				// subtract current velocity since flock includes current craft
				// and we didn'exclude it when it was orginally summed
				var exculidngHeadingSum = flock.HeadingSum - craftForwardDirection;
				// subtract from count by one to exclude current craft
				var flockAverageHeading = exculidngHeadingSum / (flockMemberCount - 1);

				return flockAverageHeading.Normalized();
			}
			return Vector3.Zero;
		}

		// lifted from Craig Reynolds' OpenSteer
		public static Vector3 Separation(Boids.Flock flock, Vector3 currentPosition) {
			var steering = Vector3.Zero;
			foreach (var craft in flock) {
				// add in steering contribution
				// (opposite of the offset direction, divided once by distance
				// to normalize, divided another time to get 1/d falloff)
				var relativePosition = craft.CraftActual.GlobalTranslation() - currentPosition;
				var offsetSquared = relativePosition.LengthSquared();
				if (offsetSquared.IsZero()) continue;
				steering -= relativePosition / offsetSquared;
			}
#if DEBUG
			// flock.DebugDraw().Call("draw_line_3d", currentPosition, flockAverageCenter, new Color(0, 1, 0));
			flock.DebugDraw().Call("draw_ray_3d", currentPosition, steering.Normalized(), 200, new Color(1, 1, 1));
#endif
			return steering;
		}
	}
}