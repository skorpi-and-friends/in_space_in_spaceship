using System.Collections.Generic;
using Godot;

#if GODOT_Real_IS_DOUBLE
using Real = System.Double;
#else
using Real = System.Single;
#endif

namespace ISIS {
    public class HullCast : Spatial {
        /// <summary>
        ///     A wrapper over <see cref="Cast"/> for easy spehre casting
        /// </summary>
        public static(bool, HullCastResult?) SphereCast(
            Vector3 to,
            Vector3 from,
            Real radius,
            PhysicsDirectSpaceState space,
            int collisionMask,
            bool collideWithBodies = true,
            bool collideWithAreas = false
        ) {
            var parameters = new PhysicsShapeQueryParameters {
            Transform = new Transform(Basis.Identity, from),
            CollisionMask = collisionMask,
            CollideWithAreas = collideWithAreas,
            CollideWithBodies = collideWithBodies,
            };
            parameters.SetShape(new SphereShape { Radius = radius });
            return Cast(parameters, to, space);
        }
        /// <summary>
        ///     Perform a hull cast according to the given parameteres.
        /// </summary>
        /// <remarks>
        ///     The wrapper methods will construct a new parameter every frame. You can use this if you want
        ///     to reuse it.
        /// </remarks>
        public static(bool, HullCastResult?) Cast(
            PhysicsShapeQueryParameters parameters,
            Vector3 motion,
            PhysicsDirectSpaceState space
        ) {
            var castResult = space.CastMotion(parameters, motion);

            if (motion.Equals(parameters.Transform.origin))
                GD.Print($"HullCast: to === from\n cast result: {castResult}");

            // Godot physics returns empty array when shape can't move
            if (castResult.Count > 0 && ((Real) castResult[0]).EqualsF(1) && ((Real) castResult[1]).EqualsF(1)) {
                return (false, null);
            }
            var result = space.GetRestInfo(parameters);
            if (result.Count == 0)
                return (false, null);
            return (true, new HullCastResult {
                ColliderId = (ulong) (int) result["collider_id"],
                    LinearVelocity = (Vector3) result["linear_velocity"],
                    Normal = (Vector3) result["normal"],
                    Point = (Vector3) result["point"],
                    RID = (RID) result["rid"],
                    ShapeIndex = (int) result["shape"],
            });
        }
    }
}

/*  */