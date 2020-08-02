using Godot;
using static ISIS.Static;

namespace ISIS {

    public class CraftMind : Node {

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

        public(bool, RigidBody) IsCraftMaster(object instance) {
            if (!(instance is Godot.RigidBody godotObject))
                return (false, null);

            var craftMasterScript = GD.Load<GDScript>("res://scripts/crafts/craft_master.gd");
            if (!Static.IsInstanceOfGDScript(godotObject, craftMasterScript))
                return (false, null);

            return (true, godotObject);
        }

        /// Assumes passed craft is already verified to be a craft
        public Godot.Object GetCraftState(Godot.Object craft) {
            return (Godot.Object) ((Godot.Object) craft.Get("engine")).Get("state");
        }
    }
}