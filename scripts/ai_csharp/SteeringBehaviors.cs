using System;
using Godot;
using static ISIS.Static;

#if GODOT_Real_IS_DOUBLE
using Real = System.Double;
#else
using Real = System.Single;
#endif

namespace ISIS {
	public static partial class SteeringBehaviors {
		/* Pure steering behavior functions */
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
			var toTarget = targetPosition - currentPosition;
			var distanceToTarget = toTarget.Length();
			var timeToTargetPosition = distanceToTarget / travelSpeed;
			// time to target intercept position

			return targetPosition + (timeToTargetPosition * targetVelocity);
		}
		/* public static Vector3 AvoidObstacle(Collider obstacle, Transform currentTransform) {
			{
				// linearInputModifier.z *= -.5f; // brake

				// add the opposite of the collision point vector
				//toPosition -= (info.point * collisionAvoidanceVectorMultiplier);

				var obstacleCenter = obstacle.bounds.center;

				var toCenterOfObstacle = obstacleCenter - currentTransform.position;
				var toCenterAlongForward = currentTransform.forward * Vector3.Dot(toCenterOfObstacle, currentTransform.forward);

				var fromCenterToForwardAxis = toCenterOfObstacle - toCenterAlongForward;

				return -fromCenterToForwardAxis;
				// Debug.DrawRay(currentTransform.position, toAvoidPosition * 2, Color.blue);
				//toRotation = toPosition -= (_latestRaycastHitInfo.point);

				//toPosition = -fromForwardToCenter;
			}
		} */

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
			// our goal will be offset from our path distance by this amount
			var pathDistanceOffset = direction * predictionTime * currentVelocity.Length();

			// predict our future position
			var futurePosition = currentPosition + (predictionTime * currentVelocity);

			// measure distance along path of our current and predicted positions
			var currentPathDistance = distanceOfPointAlongPath(currentPosition);
			var futurePathDistance = distanceOfPointAlongPath(futurePosition);

			// are we facing in the correction direction?
			var rightway = (pathDistanceOffset > 0) ?
				(currentPathDistance < futurePathDistance) :
				(currentPathDistance > futurePathDistance);

			// find the point on the path nearest the predicted future position
			var onPath = closestPointOnPathToPoint(futurePosition);

			var distanceFromPathSq = onPath.DistanceSquaredTo(futurePosition);

			// no steering is required if (a) our future position is inside
			// the path tube and (b) we are facing in the correct direction
			if (rightway && (distanceFromPathSq < requredProximity * requredProximity)) {
				// all is well, return zero steering
				return Vector3.Forward;
			} else {
				// otherwise we need to steer towards a target point obtained
				// by adding pathDistanceOffset to our current path position

				float targetPathDistance = currentPathDistance + pathDistanceOffset;
				var target = pathDistanceToPoint(targetPathDistance);

				// return steering to seek target on path
				return SeekPosition(currentPosition, target);
			}
		}
		/* public static Vector3 FollowPath(
			Vector3 currentPosition,
			Vector3 currentVelocity,
			Real requredProximity,
			Func<Vector3, Vector3> closestPointOnPathToPoint,
			Real predictionTime
		) {
			// predict our future position
			var futurePosition = currentPosition + predictionTime * currentVelocity;

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
		} */
	}
}