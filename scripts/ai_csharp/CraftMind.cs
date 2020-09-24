using Godot;
using GreenBehaviors;
using GreenBehaviors.Composite;
using GreenBehaviors.Decorator;
using GreenBehaviors.LeafLambda;
using static ISIS.Static;
// using SteeringRoutineResult = System.ValueTuple<Godot.Vector3, Godot.Vector3, string>;

namespace ISIS.Minds {
    // KEEP THIS CLASS SIMPLE BOYO
    public partial class CraftMind : Godot.Node {
        public RigidBody Craft => GetParent<RigidBody>();
        public Boid Presence => GetNode<Boid>("../Boid");
#if DEBUG
        [Export] public string ActiveRoutineDesc;
#endif
        public ScanPresence Target { get; set; }
        public SteeringRoutine ActiveRoutine { get; set; }

        public override void _Process(float delta) {
            base._Process(delta);
            if (ActiveRoutine != null) {
                var state = GetCraftState(Craft);
                var(linearInput, angularInput) = ActiveRoutine.Invoke(Craft.GlobalTransform, state);
                state.SetCraftInput(linearInput, angularInput);
            }
        }

        public virtual void DisableAutoPilot() {
#if DEBUG
            ActiveRoutineDesc = $"NO ACTIVE ROUTINE";
#endif
            ActiveRoutine = null;
        }

        public virtual void InterceptSetTarget() {
            if (Target == null) {
                return;
            }
#if DEBUG
            ActiveRoutineDesc = $"Intercepting Target {Target.Name}";
#endif
            ActiveRoutine = SteeringRoutines.InterceptRoutine(Target);
        }
        public virtual void EliminateSetTarget(in DecoratorNode craftInputRoutineWrapper) {
            if (Target == null) {
                return;
            }
#if DEBUG
            ActiveRoutineDesc = $"Eliminating Target {Target.Name}";
#endif
            ActiveRoutine = SteeringRoutines.AttackPersueRoutineClosure(in craftInputRoutineWrapper,
                Craft,
                Target
            );
        }
        public virtual void FollowPath(Path path) {
#if DEBUG
            ActiveRoutineDesc = $"Following Path {path}";
#endif
            ActiveRoutine = SteeringRoutines.FollowPathRoutine(path);
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

        /// <summary>
        /// Assumes passed craft is already verified to be a craft
        /// </summary>
        public static CraftStateWrapper GetCraftState(Godot.Object craft) {
            return new CraftStateWrapper((Godot.Object) ((Godot.Object) craft.Get("engine")).Get("state"));
        }

        public static void FirePrimaryWeapons(Godot.Object craft) {
            ((Godot.Object) craft.Get("arms")).Call("activate_primary");
        }
    }
    #endregion
}