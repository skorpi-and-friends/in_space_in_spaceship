using Godot;

#if GODOT_Real_IS_DOUBLE
using Real = System.Double;
#else
using Real = System.Single;
#endif

namespace ISIS.Minds.SteeringBehaviors {
    public static class Utility {
        public static bool IsInCroshair(
            Transform currentTransform,
            ScanPresence quarry
        ) {
            var quarryRigidbody = (RigidBody) ((quarry as Boid)?.GetBody());

            Real targetAngularProximity;

            // if target is moving
            if (quarryRigidbody != null) {
                // find position where weapons can hit mark
                var intereptPosition = SteeringBehaviors.FindInterceptionPosition(
                    currentTransform.origin,
                    500, // use weapon average velocity 
                    quarry.GlobalTransform.origin,
                    quarryRigidbody.LinearVelocity
                );

                // cacluate angular proximity
                targetAngularProximity = currentTransform.basis.z.AngleTo(
                    intereptPosition - currentTransform.origin
                );
            } else {
                // for immotile targets
                targetAngularProximity = currentTransform.basis.z.AngleTo(
                    quarry.GlobalTransform.origin - currentTransform.origin
                );
            }
            return targetAngularProximity < 0.001 * Static.DegreeToRadian;
        }
    }
}