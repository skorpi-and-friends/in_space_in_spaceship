using Godot;
using static ISIS.Static;

#if GODOT_Real_IS_DOUBLE
using Real = System.Double;
#else
using Real = System.Single;
#endif

namespace ISIS.Minds.SteeringBehaviors {
    public static partial class SteeringRoutines {
        public static LinearRoutineClosure InterceptRoutine(ScanPresence quarry) {
            // const string name = nameof(InterceptRoutine);
            var quarryRigidbody = (RigidBody) ((quarry as Boid)?.GetBody());

            return (Transform currentTransform, CraftStateWrapper currentState) => {
                var linearVLimit = currentState.LinearVLimit;
                return quarryRigidbody != null ?
                    SteeringBehaviors.InterceptObject(
                        currentTransform.origin, linearVLimit.z, quarryRigidbody) :
                    SteeringBehaviors.SeekPosition(
                        currentTransform.origin, quarry.GlobalTransform.origin);
            };
        }
    }
}