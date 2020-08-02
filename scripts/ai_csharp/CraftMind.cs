using Godot;
using static ISIS.Static;

namespace ISIS {
    public partial class CraftMind : Node {

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
    }

    public delegate(Vector3 linearInput, Vector3 angularInput) SteeringRoutine(Transform currentTransform,
        Godot.Object currentState);

    public partial class CraftMind {

        public static SteeringRoutine InterceptRoutine(ScanPresence quarry) {
            var quarryRigidbody = (RigidBody) ((quarry as Boid)?.GetBody());

            return (Transform currentTransform, Godot.Object currentState) => {
                var linearVLimit = (Vector3) currentState.Get("linear_v_limit");
                var steerVector = quarryRigidbody != null ?
                    SteeringBehaviors.InterceptObject(
                        currentTransform.origin, linearVLimit.z, quarryRigidbody) :
                    SteeringBehaviors.SeekPosition(
                        currentTransform.origin, quarry.Translation);

                return (
                    currentTransform.TransformVectorInv(steerVector) * linearVLimit,
                    FaceDirectionAngularInput(steerVector.Normalized(), currentTransform)
                );
            };

        }
    }

}