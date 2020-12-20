using Godot;
using static ISIS.Static;

#if GODOT_Real_IS_DOUBLE
using Real = System.Double;
#else
using Real = System.Single;
#endif

namespace ISIS.Minds.SteeringBehaviors {
    public static partial class SteeringRoutines {
        /// <summary>
        /// Avoids obstacles by casting rays in the direction of velocity and if obstacle is detected,
        /// calculates the steering vector from the obstacle's radius.
        /// For example, if an obstacle with a diameter of 30Ms is straight ahead, it calculates
        /// a vector that points 15+ meteres to one side or another.
        /// </summary>
        /// <remarks>
        ///     This requres every obstacle to have spherical colliders.
        /// </remarks>
        public static LinearRoutineClosure AvoidObstacle(
            RigidBody craft,
            Real predectionTimeSeconds = 5
        ) {
            var raycast = new RayCast {
            CollideWithBodies = false,
            CollideWithAreas = true,
            Enabled = true,
            CollisionMask = ObstacleSilhouette.CollisionSillhoeteLayer
            };

            raycast.Name = "ObstacleDetectionRaycast";
            craft.CallDeferred("add_child", raycast);

            return (Transform currentTransform, CraftStateWrapper currentState) => {
                raycast.CastTo = currentState.LinearVelocty * predectionTimeSeconds;
                if (raycast.IsColliding()) {
                    var obstacle = (ObstacleSilhouette) raycast.GetCollider();
                    var avoidVector = SteeringBehaviors.AvoidObstacle(
                        obstacle.GlobalTranslation(),
                        obstacle.Radius,
                        currentTransform
                    );
#if DEBUG 
                    // draw blue line in direction of avoid vector
                    craft.DebugDraw().Call("draw_line_3d", currentTransform.origin, avoidVector * currentState.LinearVelocty.Length(), new Color(0, 0, 1));
#endif
                    return avoidVector;
                }
                return Vector3.Zero;
            };
        }

        public static LinearRoutineClosure AvoidObstacleGoOpposite(
            RigidBody craft,
            Vector3 craftExtents,
            Real predectionTimeSeconds = 5,
            System.Collections.Generic.IEnumerable<RID> obstacleExculsionList = null
        ) {
            var halfExtents = craftExtents * .5f;

            var halfWidestDimension = halfExtents.x > halfExtents.y ? halfExtents.x : halfExtents.y;
            if (halfExtents.z > halfWidestDimension)
                halfWidestDimension = halfExtents.z;

            var physicsSpace = craft.GetWorld().DirectSpaceState;

            var raycastExclusionList = obstacleExculsionList != null ?
                new Godot.Collections.Array(obstacleExculsionList) :
                new Godot.Collections.Array();

            raycastExclusionList.Add(craft.GetRid());

            bool raycaster(Vector3 from, Vector3 to) {
                var collisionResult = physicsSpace.IntersectRay(
                    from,
                    to,
                    raycastExclusionList,
                    // collisionMask :  ObstacleSilhouette.CollisionSillhoeteLayer,
                    collideWithBodies : true,
                    collideWithAreas : true
                );
                return collisionResult.Count > 0;
            }

            return (Transform currentTransform, CraftStateWrapper currentState) => {
                var localVelocity = currentState.LinearVelocty;
                if (localVelocity.LengthSquared().EqualsF(0)) {
                    return Vector3.Zero;
                }

                // we'll have to expand the velocity by half our craft extents 
                // to adjust collision anticipation for craft size.
                // A 50M vehicle will collide far sooner than a 5M vehicle.
                var castTo = new Vector3(
                    localVelocity.x.Sign() * (localVelocity.x.Abs() + halfExtents.x),
                    localVelocity.y.Sign() * (localVelocity.y.Abs() + halfExtents.y),
                    localVelocity.z.Sign() * (localVelocity.z.Abs() + halfExtents.z)
                );
                castTo *= predectionTimeSeconds;

                // do the transformation last
                castTo = currentTransform.TransformPoint(castTo);

                if (!raycaster(currentTransform.origin, castTo)) {
#if DEBUG 
                    craft.DebugDraw().Call("draw_line_3d", currentTransform.origin, castTo, new Color(0, 1, 0));
#endif
                    return Vector3.Zero;
                }
#if DEBUG 
                craft.DebugDraw().Call("draw_line_3d", currentTransform.origin, castTo, new Color(1, 0, 0));
#endif
                var avoidVector = -castTo.Normalized();
#if DEBUG 
                craft.DebugDraw().Call("draw_line_3d", currentTransform.origin, avoidVector * castTo.Length(), new Color(0, 0, 1));
#endif
                return avoidVector;
            };
        }
        /// <summary>
        /// Avoids obstacles by casting rays in the direction of velocity and if obstacle is detected,
        /// cast's rays in different directions until a way out is found. These rays are cast in
        /// ever increasing angles from the direction of the velocity.
        /// </summary>
        /// <remarks>
        ///     This works with any colliders.
        /// </remarks>
        /// <seealso cref="AvoidObstacleSebLague"/>
        public static LinearRoutineClosure AvoidObstacleSebLagueRay(
            RigidBody craft,
            Vector3 craftExtents,
            Real predectionTimeSeconds = 5,
            System.Collections.Generic.IEnumerable<RID> obstacleExculsionList = null
        ) {
            var halfExtents = craftExtents * .5f;

            var halfWidestDimension = halfExtents.x > halfExtents.y ? halfExtents.x : halfExtents.y;
            if (halfExtents.z > halfWidestDimension)
                halfWidestDimension = halfExtents.z;

            var physicsSpace = craft.GetWorld().DirectSpaceState;

            var raycastExclusionList = obstacleExculsionList != null ?
                new Godot.Collections.Array(obstacleExculsionList) :
                new Godot.Collections.Array();

            raycastExclusionList.Add(craft.GetRid());

            bool raycaster(Vector3 from, Vector3 to) {
                var collisionResult = physicsSpace.IntersectRay(
                    from,
                    to,
                    raycastExclusionList,
                    // collisionMask :  ObstacleSilhouette.CollisionSillhoeteLayer,
                    collideWithBodies : true,
                    collideWithAreas : true
                );
                return collisionResult.Count > 0;
            }

            /* var raycast = new RayCast {
                CollideWithBodies = false,
                CollideWithAreas = true,
                Enabled = true,
                CollisionMask = ObstacleSilhouette.CollisionSillhoeteLayer
            };

            raycast.Name = "ObstacleDetectionRaycast";
            craft.CallDeferred("add_child", raycast); */

            return (Transform currentTransform, CraftStateWrapper currentState) => {
                var localVelocity = currentState.LinearVelocty;
                if (localVelocity.LengthSquared().EqualsF(0)) {
                    return Vector3.Zero;
                }

                // we'll have to expand the velocity by half our craft extents 
                // to adjust collision anticipation for craft size.
                // A 50M vehicle will collide far sooner than a 5M vehicle.
                var castTo = new Vector3(
                    localVelocity.x.Sign() * (localVelocity.x.Abs() + halfExtents.x),
                    localVelocity.y.Sign() * (localVelocity.y.Abs() + halfExtents.y),
                    localVelocity.z.Sign() * (localVelocity.z.Abs() + halfExtents.z)
                );
                castTo *= predectionTimeSeconds;

                // do the transformation last
                castTo = currentTransform.TransformPoint(castTo);

                // raycast.CastTo = castTo;
                // if (!raycast.IsColliding()) {
                if (!raycaster(currentTransform.origin, castTo)) {
#if DEBUG 
                    craft.DebugDraw().Call("draw_line_3d", currentTransform.origin, castTo, new Color(0, 1, 0));
#endif
                    return Vector3.Zero;
                }
#if DEBUG 
                craft.DebugDraw().Call("draw_line_3d", currentTransform.origin, castTo, new Color(1, 0, 0));
#endif
                var avoidVector = SteeringBehaviors.AvoidObstacleSebLague(
                    localVelocity,
                    raycaster,
                    currentTransform,
                    halfWidestDimension
                );
#if DEBUG 
                craft.DebugDraw().Call("draw_line_3d", currentTransform.origin, avoidVector * castTo.Length(), new Color(0, 0, 1));
#endif
                return avoidVector;
            };
        }

        /// <summary>
        /// Exactly like <see cref="AvoidObstacleSebLagueRay"/> but uses sphere casts to detect collisions instead
        /// of rays.
        /// </summary>
        /// <seealso cref="AvoidObstacleSebLagueRay"/>
        public static LinearRoutineClosure AvoidObstacleSebLague(
            RigidBody craft,
            Vector3 craftExtents,
            Real predectionTimeSeconds = 5
        ) {
            var halfExtents = craftExtents * .5f;

            var halfWidestDimension = halfExtents.x > halfExtents.y ? halfExtents.x : halfExtents.y;
            if (halfExtents.z > halfWidestDimension)
                halfWidestDimension = halfExtents.z;

            var physicsSpace = craft.GetWorld().DirectSpaceState;

            var raycastExclusionList = new Godot.Collections.Array { craft.GetRid() };

            bool raycaster(Vector3 from, Vector3 to) {
                var collisionResult = physicsSpace.IntersectRay(
                    from,
                    to,
                    raycastExclusionList,
                    ObstacleSilhouette.CollisionSillhoeteLayer,
                    collideWithBodies : false,
                    collideWithAreas : true);
                return collisionResult.Count > 0;
            }

            var parameters = new PhysicsShapeQueryParameters {
                CollideWithBodies = false,
                CollideWithAreas = true,
                CollisionMask = ObstacleSilhouette.CollisionSillhoeteLayer,
                Exclude = raycastExclusionList,
            };
            parameters.SetShape(new SphereShape { Radius = halfWidestDimension });

            return (Transform currentTransform, CraftStateWrapper currentState) => {
                var localVelocity = currentState.LinearVelocty;

                // early return
                if (localVelocity.LengthSquared().EqualsF(0)) {
                    return Vector3.Zero;
                }

                // we'll have to expand the velocity by half our craft extents 
                // to adjust collision anticipation for craft size.
                // A 50M vehicle will collide far sooner than a 5M vehicle.
                var castTo = new Vector3(
                    localVelocity.x.Sign() * (localVelocity.x.Abs() + halfExtents.x),
                    localVelocity.y.Sign() * (localVelocity.y.Abs() + halfExtents.y),
                    localVelocity.z.Sign() * (localVelocity.z.Abs() + halfExtents.z)
                );
                castTo *= predectionTimeSeconds;
                // do the transformation last
                castTo = currentTransform.TransformPoint(castTo);

                parameters.Transform = currentTransform;
                var(isColliding, result) = HullCast.Cast(parameters, castTo, physicsSpace);
                if (!isColliding) {
#if DEBUG 
                    craft.DebugDraw().Call("draw_line_3d", currentTransform.origin, castTo, new Color(0, 1, 0));
#endif
                    return Vector3.Zero;
                }
#if DEBUG 
                craft.DebugDraw().Call("draw_line_3d", currentTransform.origin, castTo, new Color(1, 0, 0));
#endif
                var avoidVector = SteeringBehaviors.AvoidObstacleSebLague(
                    localVelocity,
                    raycaster,
                    currentTransform,
                    halfWidestDimension
                );
#if DEBUG 
                craft.DebugDraw().Call("draw_line_3d", currentTransform.origin, avoidVector * castTo.Length(), new Color(0, 0, 1));
#endif
                return avoidVector;
            };
        }
    }
}