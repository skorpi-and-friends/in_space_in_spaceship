using Godot;
using static ISIS.Static;

#if GODOT_Real_IS_DOUBLE
using Real = System.Double;
#else
using Real = System.Single;
#endif

namespace ISIS.Minds.SteeringBehaviors {
    public static partial class SteeringRoutines {
        public static LinearRoutineClosure Separation(Boids.Flock flock) {
            // const string name = nameof(InterceptRoutine);
            return (Transform currentTransform, CraftStateWrapper _) =>
                SteeringBehaviors.Separation(flock, currentTransform.origin);
        }
    }
}