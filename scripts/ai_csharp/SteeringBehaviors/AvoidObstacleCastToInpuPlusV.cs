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
    public class AvoidObstacleCastToInputPlusV : AvoidObstacleSebLagueRay {
        public AvoidObstacleCastToInputPlusV(RigidBody craft, Vector3 craftExtents, float predectionTimeSeconds = 5, IEnumerable<RID> obstacleExculsionList = null):
            base(craft, craftExtents, predectionTimeSeconds, obstacleExculsionList) { }
        protected override(bool collisionPredicted, Vector3 detectionVector) PredictCollision(Transform currentTransform, CraftStateWrapper currentState) {
            var localVelocity = currentState.LinearVelocty;
            var speed = localVelocity.Length();
            var localVelocityDirection = localVelocity / speed;
            var castTo = localVelocityDirection + currentState.LinearInput.Normalized();
            castTo = castTo.Normalized();

            castTo *= speed;

            // we'll have to expand the cast vector by half our craft extents 
            // to adjust collision anticipation for craft size.
            // A 50M vehicle will collide far sooner than a 5M vehicle.
            castTo = new Vector3(
                castTo.x.Sign() * (castTo.x.Abs() + _halfExtents.x),
                castTo.y.Sign() * (castTo.y.Abs() + _halfExtents.y),
                castTo.z.Sign() * (castTo.z.Abs() + _halfExtents.z)
            );

            castTo *= _predectionTimeSeconds;

            // do the transformation last
            castTo = currentTransform.TransformPoint(castTo);

            return (Raycaster(currentTransform.origin, castTo), castTo);
        }
    }
}