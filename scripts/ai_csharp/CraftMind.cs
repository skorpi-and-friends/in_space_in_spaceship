using Godot;
using GreenBehaviors;
using GreenBehaviors.Composite;
using GreenBehaviors.Decorator;
using GreenBehaviors.LeafLambda;
using static ISIS.Static;

namespace ISIS.Minds {

    public partial class CraftMind : Godot.Node {
        public RigidBody Craft => GetParent<RigidBody>();
        public Boid Presence => GetNode<Boid>("../Boid");

        public ScanPresence Target { get; set; }
        public SteeringRoutine ActiveRoutine { get; set; }

        public override void _Process(float delta) {
            base._Process(delta);
            if (ActiveRoutine != null) {
                var state = GetCraftState(Craft);
                var input = ActiveRoutine.Invoke(Craft.GlobalTransform, state);
                SetCraftInput(state, input);
            }
        }

        public virtual void InterceptSetTarget() {
            if (Target == null) {
                return;
            }
            ActiveRoutine = InterceptRoutine(Target);
        }
        public virtual void EliminateSetTarget(in DecoratorNode craftInputRoutineWrapper) {
            if (Target == null) {
                return;
            }
            ActiveRoutine = AttackPersueRoutineClosure(in craftInputRoutineWrapper,
                Craft,
                Target
            );
        }
    }

    #region UTILITITES
    public partial class CraftMind {

        public static(bool, CraftMind) IsMindfulCraft(object instance) {
            var(isCraftMaster, craft) = IsCraftMaster(instance);
            if (!isCraftMaster) {
                return (false, null);
            }
            return IsMindful(craft);

        }

        public static(bool, CraftMind) IsMindful(RigidBody craft) {
            var mind = craft.GetNodeOrNull<CraftMind>("Mind");
            if (mind == null) {
                return (false, null);
            }
            return (true, mind);
        }

        public static(bool, RigidBody) IsCraftMaster(object instance) {
            if (!(instance is Godot.RigidBody godotObject))
                return (false, null);

            var craftMasterScript = GD.Load<GDScript>("res://scripts/crafts/craft_master.gd");
            if (!Static.IsInstanceOfGDScript(godotObject, craftMasterScript))
                return (false, null);

            return (true, godotObject);
        }

        /// Assumes passed craft is already verified to be a craft
        public static Godot.Object GetCraftState(Godot.Object craft) {
            return (Godot.Object) ((Godot.Object) craft.Get("engine")).Get("state");
        }

        public static void SetLinearInput(Godot.Object state, Vector3 linearInput) {
            state.Set("linear_input", linearInput);
        }

        public static void SetAngularInput(Godot.Object state, Vector3 angularInput) {
            state.Set("angular_input", angularInput);
        }

        public static void SetCraftInput(Godot.Object state, (Vector3, Vector3) input) {
            SetLinearInput(state, input.Item1);
            SetAngularInput(state, input.Item2);
        }

        public static void FirePrimaryWeapons(Godot.Object craft) {
            ((Godot.Object) craft.Get("arms")).Call("activate_primary");
        }
    }
    #endregion

    #region STEERING ROUTINES
    public delegate(Vector3 linearInput, Vector3 angularInput) SteeringRoutine(Transform currentTransform,
        Godot.Object currentState);

    public partial class CraftMind {
        protected static Vector3 FacePositionAngularInput(Vector3 position, Transform currentTransform) =>
            FaceLocalDirectionAngularInput(currentTransform.basis.XformInv(position - currentTransform.origin));
        protected static Vector3 FaceDirectionAngularInput(Vector3 direction, Transform currentTransform) =>
            FaceLocalDirectionAngularInput(currentTransform.basis.XformInv(direction));

        protected static Vector3 FaceLocalDirectionAngularInput(Vector3 direction) {
            var temp = BasisFacingDirection(direction).GetEuler();
            return new Vector3(
                temp.x.Sign() * DeltaAngleRadians(0f, temp.x).Abs(),
                temp.y.Sign() * DeltaAngleRadians(0f, temp.y).Abs(),
                temp.z.Sign() * DeltaAngleDegrees(0f, temp.z).Abs()
            );
        }

        internal static SteeringRoutine InterceptRoutine(ScanPresence quarry) {
            var quarryRigidbody = (RigidBody) ((quarry as Boid)?.GetBody());

            return (Transform currentTransform, Godot.Object currentState) => {
                var linearVLimit = (Vector3) currentState.Get("linear_v_limit");
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

        internal static SteeringRoutine AttackPersueRoutineClosure(in DecoratorNode wrappingNode,
            RigidBody craft,
            ScanPresence quarry
        ) {

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

            var attackingRange = 1000;

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
                        _ => false
                    )
                ).AddChild(
                    new PrioritizedSelector(
                        "AttackPersue : Core")
                    .AddChild(
                        new Monitored(
                            // name
                            "AttackPersue : Intercept Target",
                            // guard node 
                            "AttackPersue : Is Target Beyond Fire Range",
                            (_) => {
                                var distanceToTargetSquared = (quarry.GlobalTransform.origin - craft.GlobalTransform.origin).LengthSquared();
                                return distanceToTargetSquared > (attackingRange * attackingRange);
                            },
                            // chargeNode
                            new Action(
                                // name
                                "AttackPersue : Intercept Wrapper",
                                // action
                                _ => NodeState.Running,
                                // start
                                _ => {
                                    activeSubroutine = interceptSubroutine;
                                }
                            )
                        )
                    ).AddChild(
                        new Action(
                            // name
                            "AttackPersue : Attack Wrapper",
                            // action
                            _ => attackSubtree.FullTick(),
                            // start
                            _ => {
                                // attackSubtree.Start();
                                activeSubroutine = attackSubroutine;
                            }
                        )
                    )
                );

                wrappingNode.SetChild(attackPersueTree);
            }

            return (Transform currentTransform, Godot.Object currentState) => {
                return activeSubroutine(currentTransform, currentState);
            };
        }

        internal static SteeringRoutine GetAttackPersueRoutineBlackboard(in DecoratorNode wrappingNode,
            RigidBody craft,
            ScanPresence quarry
        ) {
            throw new System.NotImplementedException();
        }

        internal static SteeringRoutine AttackRoutine(in DecoratorNode wrappingNode,
            RigidBody craft,
            ScanPresence quarry
        ) {
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
                "Attack : Core"
            ).AddChild(
                new PrioritizedSelector(
                    $"Attack : Choose Craft Input Routine FSM",
                    new Monitored(
                        // name
                        $"Attack : Target Ahead Gurad",
                        // guard name
                        $"Attack : Is Target Ahead",
                        // guard callback
                        (_) => {
                            targetDirection = GetGeneralRelativeDirectionOfTransforms(
                                craft.GlobalTransform, quarry.GlobalTransform);
                            return targetDirection == GeneralRelativeDirection.Ahead;
                        },
                        // charge
                        new Action(
                            "Attack : Taget Ahead - Line Up Shot",
                            (_) => {
                                if (IsInCroshair(craft.GlobalTransform, quarry))
                                    FirePrimaryWeapons(craft);
                                return NodeState.Running;
                            },
                            // start callback
                            (_) => {
                                GD.Print("Target Is Ahead: Lining Up Shot");
                                activeSubroutine = lineUpShotRoutine;
                            }
                        )
                    ),
                    new Monitored(
                        $"Attack : Target Aside Gurad",
                        $"Attack : Is Target Aside",
                        (_) => targetDirection == GeneralRelativeDirection.Aside, // targetDirection was caclulated earlier
                        new Action(
                            "Attack : Target Aside",
                            // tick callback
                            (_) => NodeState.Running,
                            // start callback
                            (_) => {
                                GD.Print("Target Is Aside");
                                activeSubroutine = loopSubroutine;
                            }
                        )
                    ),
                    new Action(
                        "Attack : Target Behind",
                        (_) => {
                            return NodeState.Running;
                        },
                        (_) => {
                            GD.Print("Target Is Behind");
                            activeSubroutine = loopSubroutine;
                        }
                    )
                )
            );
            wrappingNode.SetChild(attackRoutineTree);

            return (Transform currentTransform, Godot.Object currentState) => {
                return activeSubroutine(currentTransform, currentState);
            };
        }

        internal static SteeringRoutine LineUpShotRoutine(
            ScanPresence quarry
        ) {

            var quarryRigidbody = (RigidBody) ((quarry as Boid)?.GetBody());
            var averageWeaponVelocity = 500; //_craft.ArmsManager.AverageFixMountedProjectileVelocity;

            return (Transform currentTransform, Godot.Object currentState) => {
                var quarryVelocity = quarryRigidbody != null ? quarryRigidbody.LinearVelocity : Vector3.Zero;
                var targetPosition = SteeringBehaviors.FindInterceptionPosition(
                    currentTransform.origin,
                    averageWeaponVelocity,
                    quarry.GlobalTransform.origin,
                    quarryVelocity
                );

                var linearVLimit = (Vector3) currentState.Get("linear_v_limit");
                return (
                    SteeringBehaviors.SeekPosition(currentTransform.origin, targetPosition) * linearVLimit,
                    FacePositionAngularInput(targetPosition, currentTransform)
                );
            };
        }

        internal static bool IsInCroshair(
            Transform currentTransform,
            ScanPresence quarry
        ) {

            var quarryRigidbody = (RigidBody) ((quarry as Boid)?.GetBody());

            float targetAngularProximity;

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

    #endregion
}