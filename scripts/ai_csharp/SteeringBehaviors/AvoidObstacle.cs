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
        ///     Avoids obstacles by casting rays in the direction of velocity and if obstacle is detected,
        ///     calculates the steering vector from the obstacle's radius.
        ///     For example, if an obstacle with a diameter of 30Ms is straight ahead, it calculates
        ///     a vector that points 15+ meteres to one side or another.
        /// </summary>
        /// <remarks>
        ///     This requres every obstacle to have spherical colliders in the <see cref="ObstacleSilhouette.CollisionSillhoeteLayer"/>
        /// </remarks>
        public static LinearRoutineClosure AvoidObstacleSilhottes(
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

            (bool, Node) raycaster(Vector3 from, Vector3 to) {
                var castResult = physicsSpace.IntersectRay(
                    from,
                    to,
                    raycastExclusionList,
                    ObstacleSilhouette.CollisionSillhoeteLayer,
                    collideWithBodies : false,
                    collideWithAreas : true
                );
                var hasIntersected = castResult.Count > 0;
                return (
                    hasIntersected,
                    hasIntersected ? castResult["collider"] as Godot.Node : null
                );
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

                var(isColliding, collider) = raycaster(currentTransform.origin, castTo);
                if (isColliding) {
                    var obstacle = (ObstacleSilhouette) collider;
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
                var avoidVector = SteeringBehaviors.AvoidObstacleSebLague(
                    castTo, // velocity direction (will be normalized within)
                    raycastDistance: (localVelocity.Length() * predectionTimeSeconds) + halfWidestDimension,
                    raycaster,
                    currentTransform
                );
#if DEBUG 
                craft.DebugDraw().Call("draw_line_3d", currentTransform.origin, avoidVector * castTo.Length(), new Color(0, 0, 1));
#endif
                return avoidVector;
            };
        }

        public static LinearRoutineClosure AvoidObstacleCastToNextInput(
            LinearRoutineClosure mainBehavior,
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
                    collideWithBodies : true,
                    collideWithAreas : true
                );
                return collisionResult.Count > 0;
            }

            return (Transform currentTransform, CraftStateWrapper currentState) => {
                var mainBehaviorOutput = mainBehavior.Invoke(currentTransform, currentState);

                var localVelocity = currentState.LinearVelocty;
                var speedSquared = localVelocity.LengthSquared();
                if (speedSquared.EqualsF(0)) {
                    return mainBehaviorOutput;
                }

                var speed = Mathf.Sqrt(speedSquared);

                var castTo = mainBehaviorOutput.Normalized() * speed;

                // we'll have to expand the cast vector by half our craft extents 
                // to adjust collision anticipation for craft size.
                // A 50M vehicle will collide far sooner than a 5M vehicle.
                castTo = new Vector3(
                    castTo.x.Sign() * (castTo.x.Abs() + halfExtents.x),
                    castTo.y.Sign() * (castTo.y.Abs() + halfExtents.y),
                    castTo.z.Sign() * (castTo.z.Abs() + halfExtents.z)
                );

                castTo *= predectionTimeSeconds;

                // do the transformation last
                // castTo = currentTransform.TransformPoint(castTo);
                castTo = currentTransform.origin + castTo;

                if (!raycaster(currentTransform.origin, castTo)) {
#if DEBUG 
                    craft.DebugDraw().Call("draw_line_3d", currentTransform.origin, castTo, new Color(0, 1, 0));
#endif
                    return mainBehaviorOutput;
                }
#if DEBUG 
                craft.DebugDraw().Call("draw_line_3d", currentTransform.origin, castTo, new Color(1, 0, 0));
#endif
                var avoidVector = SteeringBehaviors.AvoidObstacleSebLague(
                    castTo, // velocity direction (will be normalized within)
                    (speed * predectionTimeSeconds) + halfWidestDimension,
                    raycaster,
                    currentTransform,
                    craft
                );
#if DEBUG 
                craft.DebugDraw().Call("draw_line_3d", currentTransform.origin, avoidVector * castTo.Length() * 2, new Color(0, 0, 1));
#endif
                return avoidVector;
            };
        }

        /// <summary>
        /// Exactly like <see cref="AvoidObstacleSebLagueRay"/> but uses sphere casts to detect collisions instead
        /// of rays.
        /// </summary>
        /// <remarks>
        ///     Only uses hull casts in the initial obstacle detection cast. Still uses rays to find a way out.
        /// </remarks>
        /// <seealso cref="AvoidObstacleSebLagueRay"/>
        public static LinearRoutineClosure AvoidObstacleSebLague(
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
                    // ObstacleSilhouette.CollisionSillhoeteLayer,
                    collideWithBodies : true,
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
                    castTo, // velocity direction (will be normalized within)
                    raycastDistance: (localVelocity.Length() * predectionTimeSeconds) + halfWidestDimension,
                    raycaster,
                    currentTransform
                );
#if DEBUG 
                craft.DebugDraw().Call("draw_line_3d", currentTransform.origin, avoidVector * castTo.Length(), new Color(0, 0, 1));
#endif
                return avoidVector;
            };
        }
    }
}