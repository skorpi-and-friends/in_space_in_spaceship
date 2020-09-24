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

namespace ISIS.Minds {
    public delegate(Vector3 linearInput, Vector3 angularInput) SteeringRoutine(Transform currentTransform,
        CraftStateWrapper currentState);

    internal static class SteeringRoutines {
        public static Vector3 FacePositionAngularInput(Vector3 position, Transform currentTransform) =>
            FaceLocalDirectionAngularInput(currentTransform.basis.XformInv(position - currentTransform.origin));
        public static Vector3 FaceDirectionAngularInput(Vector3 direction, Transform currentTransform) =>
            FaceLocalDirectionAngularInput(currentTransform.basis.XformInv(direction));

        public static Vector3 FaceLocalDirectionAngularInput(Vector3 direction) {
            var temp = BasisFacingDirection(direction).GetEuler();
            return new Vector3(
                temp.x.Sign() * DeltaAngleRadians(0f, temp.x).Abs(),
                temp.y.Sign() * DeltaAngleRadians(0f, temp.y).Abs(),
                temp.z.Sign() * DeltaAngleDegrees(0f, temp.z).Abs()
            );
        }

        public static SteeringRoutine InterceptRoutine(ScanPresence quarry) {
            // const string name = nameof(InterceptRoutine);
            var quarryRigidbody = (RigidBody) ((quarry as Boid)?.GetBody());

            return (Transform currentTransform, CraftStateWrapper currentState) => {
                var linearVLimit = (Vector3) currentState.LinearVLimit;
                var steerVector = quarryRigidbody != null ?
                    SteeringBehaviors.InterceptObject(
                        currentTransform.origin, linearVLimit.z, quarryRigidbody) :
                    SteeringBehaviors.SeekPosition(
                        currentTransform.origin, quarry.GlobalTransform.origin);

                return (
                    currentTransform.TransformVectorInv(steerVector) * linearVLimit,
                    FaceDirectionAngularInput(steerVector.Normalized(), currentTransform)
                );
            };
        }

        public static SteeringRoutine AttackPersueRoutineClosure(in DecoratorNode wrappingNode,
            RigidBody craft,
            ScanPresence quarry
        ) {
            // const string name = nameof(AttackPersueRoutineClosure);

            /*
						 |
					AttackPersue(quarry)       blackboard = [quarry, attackingRange]
						 |
				Prioritized Selector
				   /           |
				  /            |
		   IsTargetDead      Prioritized Selector
									 |            \
								m(IsFar)           \
									 |              \  
								Intecept          Attack
								(quarry)         (quarry)
				*/
            var quarryRigidbody = (RigidBody) ((quarry as Boid)?.GetBody());

            int attackingRange = 1000;

            // AbstractDecoratorNode interceptSubtree = new Checker("Intercept Subtree Wrap");
            DecoratorNode attackSubtree = new SimpleInclude("Attack Subtree Wrap");

            var attackSubroutine = AttackRoutine(in attackSubtree, craft, quarry);
            var interceptSubroutine = InterceptRoutine(quarry);

            // initially assume we're too far to attack 
            var activeSubroutine = interceptSubroutine;

            {
                // setupTree
                var attackPersueTree = new PrioritizedSelector(
                    $"AttackPersue : Attack Persue Subtree"
                ).AddChild(
                    new Conditional(
                        $"AttackPersue : Is Quarry Dead",
                        (_) => false //FIXME:
                    )
                ).AddChild(
                    new PrioritizedSelector("AttackPersue : Core")
                    .AddChild(
                        new Monitored(
                            "AttackPersue : Intercept Target",
                            // guard
                            "AttackPersue : Is Target Beyond Fire Range",
                            (_) => {
                                var distanceToTargetSquared = (quarry.GlobalTransform.origin - craft.GlobalTransform.origin).LengthSquared();
                                return distanceToTargetSquared > (attackingRange * attackingRange);
                            },
                            // charge
                            new Action(
                                "AttackPersue : Intercept Wrapper",
                                // at tick
                                (_) => NodeState.Running,
                                // at start
                                (_) => activeSubroutine = interceptSubroutine
                            )
                        )
                    ).AddChild(
                        "AttackPersue : Attack Wrapper",
                        // at tick
                        (_) => attackSubtree.FullTick(),
                        // at start
                        (_) => {
                            // attackSubtree.Start();
                            activeSubroutine = attackSubroutine;
                        }

                    )
                );

                wrappingNode.SetChild(attackPersueTree);
            }

            return (Transform currentTransform, CraftStateWrapper currentState) => {
                return activeSubroutine(currentTransform, currentState);
            };
        }

        public static SteeringRoutine GetAttackPersueRoutineBlackboard(in DecoratorNode wrappingNode,
            RigidBody craft,
            ScanPresence quarry
        ) {
            throw new System.NotImplementedException();
        }

        public static SteeringRoutine AttackRoutine(in DecoratorNode wrappingNode,
            RigidBody craft,
            ScanPresence quarry
        ) {
            // const string name = nameof(AttackRoutine);
            /*
			  --Attack(quarry)       blackboard = Static[quarry]{targetDirecton}
					 |
				   ProritizedSelect
				   /        |       \
				  /         |        \
			  M(isAhead)  M(isAside)  +
				 |          |         |
			Line Up       Loop    Brake With Flair 
			 Shot
		*/

            var lineUpShotRoutine = LineUpShotRoutine(quarry);
            var interceptSubroutine = InterceptRoutine(quarry);
            var loopSubroutine = interceptSubroutine;
            var brakeWithFlairSubroutine = interceptSubroutine;

            var activeSubroutine = interceptSubroutine;

            // var lastTargetDirection = GeneralRelativeDirection.Ahead;
            var targetDirection = GeneralRelativeDirection.Ahead;

            var attackRoutineTree = new PrioritizedSelector(
                "Attack : Core",
                new PrioritizedSelector(
                    $"Attack : Choose Craft Input Routine FSM",
                    new Monitored(
                        $"Attack : Target Ahead Gurad",
                        guardName: $"Attack : Is Target Ahead",
                        guardConditional: (_) => {
                            targetDirection = GetGeneralRelativeDirectionOfTransforms(
                                craft.GlobalTransform, quarry.GlobalTransform);
                            return targetDirection == GeneralRelativeDirection.Ahead;
                        },
                        charge : new Action(
                            "Attack : Taget Ahead - Line Up Shot",
                            tick: (_) => {
                                if (IsInCroshair(craft.GlobalTransform, quarry))
                                    CraftMind.FirePrimaryWeapons(craft);
                                return NodeState.Running;
                            },
                            start: (_) => {
                                GD.Print("Target Is Ahead: Lining Up Shot");
                                activeSubroutine = lineUpShotRoutine;
                            }
                        )
                    ),
                    new Monitored(
                        $"Attack : Target Aside Gurad",
                        guardName: $"Attack : Is Target Aside",
                        guardConditional: (_) => targetDirection == GeneralRelativeDirection.Aside, // targetDirection was caclulated earlier
                        charge : new Action(
                            "Attack : Target Aside",
                            tick: (_) => NodeState.Running,
                            start: (_) => {
                                GD.Print("Target Is Aside");
                                activeSubroutine = loopSubroutine;
                            }
                        )
                    ),
                    new Action(
                        "Attack : Target Behind",
                        tick: (_) => NodeState.Running,
                        start: (_) => {
                            GD.Print("Target Is Behind");
                            activeSubroutine = loopSubroutine;
                        }
                    )
                )
            );
            wrappingNode.SetChild(attackRoutineTree);

            return (Transform currentTransform, CraftStateWrapper currentState) => activeSubroutine(currentTransform, currentState);
        }

        public static SteeringRoutine LineUpShotRoutine(
            ScanPresence quarry
        ) {
            var quarryRigidbody = (RigidBody) ((quarry as Boid)?.GetBody());
            int averageWeaponVelocity = 500; //_craft.ArmsManager.AverageFixMountedProjectileVelocity;

            return (Transform currentTransform, CraftStateWrapper currentState) => {
                var quarryVelocity = quarryRigidbody != null ? quarryRigidbody.LinearVelocity : Vector3.Zero;
                var targetPosition = SteeringBehaviors.FindInterceptionPosition(
                    currentTransform.origin,
                    averageWeaponVelocity,
                    quarry.GlobalTransform.origin,
                    quarryVelocity
                );

                var linearVLimit = currentState.LinearVLimit;
                return (
                    SteeringBehaviors.SeekPosition(currentTransform.origin, targetPosition) * linearVLimit,
                    FacePositionAngularInput(targetPosition, currentTransform)
                );
            };
        }

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
        public static SteeringRoutine FollowPathRoutine(
            Path path,
            int requirePathProximityMeters = 10,
            PathFollowDirection followDirection = PathFollowDirection.Forward,
            int predectionTimeSeconds = 5
        ) {
            var curve = path.Curve;
            System.Func<Vector3, Vector3> closestPointOnPathToPoint =
                (point) => curve.GetClosestPoint(path.GlobalTransform.TransformVector(point));

            System.Func<Real, Vector3> pathDistanceToPoint =
                (distance) => curve.InterpolateBaked(distance);

            System.Func<Vector3, Real> distanceOfPointAlongPath =
                (point) => curve.GetClosestOffset(path.GlobalTransform.TransformVector(point));

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
                return (
                    currentTransform.TransformVectorInv(steerVector) * currentState.LinearVLimit,
                    FaceDirectionAngularInput(steerVector.Normalized(), currentTransform)
                );
            };
        }

        public static SteeringRoutine AvoidObstacleRoutine(
            RigidBody craft,
            int predectionTimeSeconds = 1
        ) {
            var raycast = new RayCast();

            raycast.CollideWithBodies = false;
            raycast.CollideWithAreas = true;
            raycast.Enabled = true;

            return (Transform currentTransform, CraftStateWrapper currentState) => {

                return (
                    Vector3.Zero,
                    Vector3.Zero
                );
            };
        }
    }
}