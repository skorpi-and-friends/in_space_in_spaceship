using System;
using Godot;
using static ISIS.Static;

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
			float travelSpeed,
			RigidBody objectRigidBody
		) => InterceptObject(currentPosition, travelSpeed, objectRigidBody.Translation, objectRigidBody.LinearVelocity);

		public static Vector3 InterceptObject(
			Vector3 currentPosition,
			float travelSpeed,
			Vector3 objectPosition,
			Vector3 objectVelocity
		) => SeekPosition(
			currentPosition,
			FindInterceptionPosition(currentPosition, travelSpeed, objectPosition, objectVelocity));

		public static Vector3 FindInterceptionPosition(
			Vector3 currentPosition,
			float travelSpeed,
			RigidBody targetRigidBody
		) => FindInterceptionPosition(currentPosition, travelSpeed, targetRigidBody.Translation, targetRigidBody.LinearVelocity);

		public static Vector3 FindInterceptionPosition(
			Vector3 currentPosition,
			float travelSpeed,
			Vector3 targetPosition,
			Vector3 targetVelocity
		) {
			// FIXME improve
			var toTarget = targetPosition - currentPosition;
			var distanceToTarget = toTarget.Length();
			var timeToTargetPosition = distanceToTarget / travelSpeed;
			// time to target intercept position

			return (targetPosition + (timeToTargetPosition * targetVelocity));
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

	}
}
