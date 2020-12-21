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
    ///     Avoids obstacles by casting rays in the direction of velocity and if obstacle is detected, goes in the opposite direction.
    /// </summary>
    /// <remarks>
    ///     This assumes you blend it's output with something else in a reasonable way.
    /// </remarks>
    public class AvoidObstacleGoOpposite : AvoidObstacleSebLagueRay {
        public AvoidObstacleGoOpposite(RigidBody craft, Vector3 craftExtents, float predectionTimeSeconds = 5, IEnumerable<RID> obstacleExculsionList = null):
            base(craft, craftExtents, predectionTimeSeconds, obstacleExculsionList) { }
        protected override Vector3 AvoidCollision(Transform _, CraftStateWrapper __, Vector3 detectionVector) {
            return -detectionVector;
        }
    }
}