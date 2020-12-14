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
        // FIXME: Attack in itself contains Intercept, we don't need to intercept first
        public static SteeringRoutineClosure AttackPersueRoutineClosure(in DecoratorNode wrappingNode,
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

            // TODO: make this dynamic
            const int attackingRange = 1000;

            // AbstractDecoratorNode interceptSubtree = new Checker("Intercept Subtree Wrap");
            DecoratorNode attackSubtree = new SimpleInclude("Attack Subtree Wrap");

            var attackSubroutine = AttackRoutine(in attackSubtree, craft, quarry);
            var interceptSubroutine = LookWhereYouGoRoutineComposer(
                InterceptRoutine(quarry)
            );

            // initially assume we're too far to attack 
            var activeSubroutine = interceptSubroutine;

            {
                // setupTree
                var attackPersueTree = new PrioritizedSelector(
                    "AttackPersue : Attack Persue Subtree"
                ).AddChild(
                    new Conditional(
                        "AttackPersue : Is Quarry Dead",
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

            return (Transform currentTransform, CraftStateWrapper currentState) =>
                activeSubroutine(currentTransform, currentState);
        }

        public static SteeringRoutineClosure AttackPersueRoutineBlackboard(in DecoratorNode wrappingNode,
            RigidBody craft,
            ScanPresence quarry
        ) {
            throw new System.NotImplementedException();
        }

        public static SteeringRoutineClosure AttackRoutine(in DecoratorNode wrappingNode,
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
            var interceptSubroutine = LookWhereYouGoRoutineComposer(InterceptRoutine(quarry));
            var loopSubroutine = interceptSubroutine;
            var brakeWithFlairSubroutine = interceptSubroutine;

            var activeSubroutine = interceptSubroutine;

            // var lastTargetDirection = GeneralRelativeDirection.Ahead;
            var targetDirection = GeneralRelativeDirection.Ahead;

            var attackRoutineTree = new PrioritizedSelector(
                "Attack : Core",
                new PrioritizedSelector(
                    "Attack : Choose Craft Input Routine FSM",
                    new Monitored(
                        "Attack : Target Ahead Gurad",
                        guardName: "Attack : Is Target Ahead",
                        guardConditional: (_) => {
                            targetDirection = GetGeneralRelativeDirectionOfTransforms(
                                craft.GlobalTransform, quarry.GlobalTransform);
                            return targetDirection == GeneralRelativeDirection.Ahead;
                        },
                        charge : new Action(
                            "Attack : Taget Ahead - Line Up Shot",
                            tick: (_) => {
                                if (Utility.IsInCroshair(craft.GlobalTransform, quarry))
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
                        "Attack : Target Aside Gurad",
                        guardName: "Attack : Is Target Aside",
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

            return (Transform currentTransform, CraftStateWrapper currentState) =>
                activeSubroutine(currentTransform, currentState);
        }

        public static SteeringRoutineClosure LineUpShotRoutine(
            ScanPresence quarry
        ) {
            var quarryRigidbody = (RigidBody) ((quarry as Boid)?.GetBody());
            const int averageWeaponVelocity = 500; //_craft.ArmsManager.AverageFixMountedProjectileVelocity;

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
                    SteeringBehaviors.SeekPosition(currentTransform.origin, targetPosition), // * linearVLimit,
                    FacePositionAngularInput(targetPosition, currentTransform)
                );
            };
        }
    }
}