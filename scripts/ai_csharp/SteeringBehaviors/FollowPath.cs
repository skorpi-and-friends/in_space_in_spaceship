using Godot;
using GreenBehaviors;
using GreenBehaviors.Composite;
using GreenBehaviors.Decorator;
using GreenBehaviors.LeafLambda;
using ISIS.Minds;
using static ISIS.Static;

#if GODOT_Real_IS_DOUBLE
using Real = System.Double;
#else
using Real = System.Single;
#endif

namespace ISIS.SteeringBehaviors {
    public static partial class SteeringRoutines {
        public enum PathFollowDirection {
            Forward = 1,
            Backward = -1
        }

        /// <summary>
        /// The craft will follow the path at max velocity.
        /// </summary>
        /// <remarks>
        ///     Use a curve bake interval of 20 or something for good performance.
        ///     The default will tank your fps. You're bound to forget to do this
        ///     but it's better to treat our input path immutable in such funcs.
        /// </remarks>
        /// <param name="path">Path to follow.</param>
        public static LinearRoutineClosure FollowPathRoutine(
            Path path,
            Real requirePathProximityMeters = 10,
            PathFollowDirection followDirection = PathFollowDirection.Forward,
            Real predectionTimeSeconds = 3
        ) {
            var curve = path.Curve;
            // closures
            Vector3 closestPointOnPathToPoint(Vector3 point) =>
                curve.GetClosestPoint(path.GlobalTransform.TransformVector(point));

            Vector3 pathDistanceToPoint(float distance) => curve.InterpolateBaked(distance);

            float distanceOfPointAlongPath(Vector3 point) => curve.GetClosestOffset(path.GlobalTransform.TransformVector(point));

            return (Transform currentTransform, CraftStateWrapper currentState) => {
                var steerVector = SteeringBehaviors.FollowPath(
                    currentTransform.origin,
                    currentState.LinearVelocty,
                    requredProximity : requirePathProximityMeters, //FIXME: parameterize
                    closestPointOnPathToPoint,
                    distanceOfPointAlongPath,
                    pathDistanceToPoint,
                    direction: (int) followDirection,
                    predictionTime : predectionTimeSeconds);
                return currentTransform.TransformVectorInv(steerVector);
            };
        }
    }
}