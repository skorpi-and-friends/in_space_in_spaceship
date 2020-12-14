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

namespace ISIS.SteeringBehaviors {
    /// <summary>
    /// Class form of <seealso cref="SteeringRoutines.AvoidObstacleSebLagueRay"/>.
    /// </summary>
    public class AvoidObstacleSebLagueRay : ISteeringRoutine {
        private readonly RigidBody _craft;
        private readonly Vector3 _halfExtents;
        private readonly Real _halfWidestDimension;
        private readonly Real _predectionTimeSeconds;
        private readonly PhysicsDirectSpaceState _physicsSpace;
        private readonly Godot.Collections.Array _raycastExclusionList;
        private readonly RayCast _raycast;

        public AvoidObstacleSebLagueRay(RigidBody craft, Vector3 craftExtents, Real predectionTimeSeconds = 5) {
            _craft = craft;
            _predectionTimeSeconds = predectionTimeSeconds;
            _raycastExclusionList = new Godot.Collections.Array { craft.GetRid() };
            _physicsSpace = craft.GetWorld().DirectSpaceState;

            _halfExtents = craftExtents * .5f;
            _halfWidestDimension = _halfExtents.x > _halfExtents.y ? _halfExtents.x : _halfExtents.y;
            if (_halfExtents.z > _halfWidestDimension)
                _halfWidestDimension = _halfExtents.z;

            _raycast = new RayCast {
                CollideWithBodies = false,
                CollideWithAreas = true,
                Enabled = true,
                CollisionMask = ObstacleSilhouette.CollisionSillhoeteLayer
            };

            _raycast.Name = "ObstacleDetectionRaycast";
            craft.CallDeferred("add_child", _raycast);
        }

        public SteeringInput CalculateSteering(Transform currentTransform, CraftStateWrapper currentState) {
            var localVelocity = currentState.LinearVelocty;
            if (localVelocity.LengthSquared().EqualsF(0)) {
                return SteeringInput.Zero;
            }

            // we'll have to expand the velocity by half our craft extents 
            // to adjust collision anticipation for craft size.
            // A 50M vehicle will collide far sooner than a 5M vehicle.
            var castTo = new Vector3(
                localVelocity.x.Sign() * (localVelocity.x.Abs() + _halfExtents.x),
                localVelocity.y.Sign() * (localVelocity.y.Abs() + _halfExtents.y),
                localVelocity.z.Sign() * (localVelocity.z.Abs() + _halfExtents.z)
            );
            castTo *= _predectionTimeSeconds;

            _raycast.CastTo = castTo;
            if (!_raycast.IsColliding()) {
#if DEBUG 
                _craft.DebugDraw().Call("draw_line_3d", currentTransform.origin, castTo, new Color(0, 1, 0));
#endif
                return SteeringInput.Zero;
            }
#if DEBUG 
            _craft.DebugDraw().Call("draw_line_3d", currentTransform.origin, castTo, new Color(1, 0, 0));
#endif
            var avoidVector = SteeringBehaviors.AvoidObstacleSebLague(
                localVelocity,
                Raycaster,
                currentTransform,
                _halfWidestDimension
            );
#if DEBUG 
            _craft.DebugDraw().Call("draw_line_3d", currentTransform.origin, avoidVector * castTo.Length(), new Color(0, 0, 1));
#endif
            return new SteeringInput(
                avoidVector,
                Vector3.Zero //  SteeringRoutines.FaceDirectionAngularInput(avoidVector, currentTransform)
            );
        }

        private bool Raycaster(Vector3 from, Vector3 to) {
            var collisionResult = _physicsSpace.IntersectRay(
                from,
                to,
                _raycastExclusionList,
                ObstacleSilhouette.CollisionSillhoeteLayer,
                collideWithBodies : false,
                collideWithAreas : true);
            return collisionResult.Count > 0;
        }
    }
}