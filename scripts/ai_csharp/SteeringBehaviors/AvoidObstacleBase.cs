using System.Collections.Generic;
using System.Diagnostics;
using Godot;
using GreenBehaviors;
using GreenBehaviors.Composite;
using GreenBehaviors.Decorator;
using GreenBehaviors.LeafLambda;
using static ISIS.Static;

#if GODOT_Real_IS_DOUBLE
using Real = System.Double;
#else
using Real = System.Single;
#endif

namespace ISIS.Minds.SteeringBehaviors {
    /// <summary>
    /// Base class for some AvoidObstacle implementations.
    /// </summary>
    public abstract class AvoidObstacleBase : ILinearRoutine {
        protected readonly RigidBody _craft;
        protected readonly Vector3 _halfExtents;
        protected readonly Real _halfWidestDimension;
        protected readonly Real _predectionTimeSeconds;
        protected readonly PhysicsDirectSpaceState _physicsSpace;
        protected readonly Godot.Collections.Array _raycastExclusionList;

        protected AvoidObstacleBase(RigidBody craft, Vector3 craftExtents, Real predectionTimeSeconds = 5,
            System.Collections.Generic.IEnumerable<RID> obstacleExculsionList = null) {
            _craft = craft;
            _predectionTimeSeconds = predectionTimeSeconds;

            _raycastExclusionList = obstacleExculsionList != null ?
                new Godot.Collections.Array(obstacleExculsionList) :
                new Godot.Collections.Array();

            _raycastExclusionList.Add(craft.GetRid());

            _physicsSpace = craft.GetWorld().DirectSpaceState;

            _halfExtents = craftExtents * .5f;
            _halfWidestDimension = _halfExtents.x > _halfExtents.y ? _halfExtents.x : _halfExtents.y;
            if (_halfExtents.z > _halfWidestDimension)
                _halfWidestDimension = _halfExtents.z;
        }

        protected abstract(bool collisionPredicted, Vector3 detectionVector) PredictCollision(Transform currentTransform, CraftStateWrapper currentState);

        protected abstract Vector3 AvoidCollision(Transform currentTransform, CraftStateWrapper currentState, Vector3 detectionVector);

        protected virtual bool Raycaster(Vector3 from, Vector3 to) {
            var collisionResult = _physicsSpace.IntersectRay(
                from,
                to,
                _raycastExclusionList,
                // collisionMask :  ObstacleSilhouette.CollisionSillhoeteLayer,
                collideWithBodies : true,
                collideWithAreas : true
            );
            return collisionResult.Count > 0;
        }
        public virtual Vector3 CalculateSteering(Transform currentTransform, CraftStateWrapper currentState) {
            var localVelocity = currentState.LinearVelocty;
            if (localVelocity.LengthSquared().EqualsF(0)) {
                return Vector3.Zero;
            }

            var(collisionPredicted, detectionVector) = PredictCollision(currentTransform, currentState);
            if (!collisionPredicted) {
#if DEBUG 
                _craft.DebugDraw().Call("draw_line_3d", currentTransform.origin, detectionVector, new Color(0, 1, 0));
#endif
                return Vector3.Zero;
            }
#if DEBUG 
            _craft.DebugDraw().Call("draw_line_3d", currentTransform.origin, detectionVector, new Color(1, 0, 0));
#endif
            var avoidVector = AvoidCollision(currentTransform, currentState, detectionVector);
#if DEBUG 
            _craft.DebugDraw().Call("draw_line_3d", currentTransform.origin, avoidVector * detectionVector.Length(), new Color(0, 0, 1));
#endif
            return avoidVector;
        }
    }
}