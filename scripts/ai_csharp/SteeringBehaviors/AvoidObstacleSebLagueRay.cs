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
    /// Class form of <seealso cref="SteeringRoutines.AvoidObstacleSebLagueRay"/>.
    /// </summary>
    public class AvoidObstacleSebLagueRay : AvoidObstacleBase {
        public AvoidObstacleSebLagueRay(RigidBody craft, Vector3 craftExtents, float predectionTimeSeconds = 5, IEnumerable<RID> obstacleExculsionList = null) : base(craft, craftExtents, predectionTimeSeconds, obstacleExculsionList) { }

        protected override(bool collisionPredicted, Vector3 detectionVector) PredictCollision(Transform currentTransform, CraftStateWrapper currentState) {
            var localVelocity = currentState.LinearVelocty;
            // we'll have to expand the velocity by half our craft extents 
            // to adjust collision anticipation for craft size.
            // A 50M vehicle will collide far sooner than a 5M vehicle.
            var castTo = new Vector3(
                localVelocity.x.Sign() * (localVelocity.x.Abs() + _halfExtents.x),
                localVelocity.y.Sign() * (localVelocity.y.Abs() + _halfExtents.y),
                localVelocity.z.Sign() * (localVelocity.z.Abs() + _halfExtents.z)
            );
            castTo *= _predectionTimeSeconds;

            // do the transformation last
            castTo = currentTransform.TransformPoint(castTo);
            return (Raycaster(currentTransform.origin, castTo), castTo);
        }

        protected override Vector3 AvoidCollision(Transform currentTransform, CraftStateWrapper currentState, Vector3 detectionVector) {
            return SteeringBehaviors.AvoidObstacleSebLague(
                detectionVector,
                (currentState.LinearVelocty.Length() * _predectionTimeSeconds) + _halfWidestDimension,
                Raycaster,
                currentTransform,
                _craft
            );
        }
    }
}