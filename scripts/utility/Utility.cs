using Godot;

#if GODOT_Real_IS_DOUBLE
using Real = System.Double;
#else
using Real = System.Single;
#endif

namespace ISIS {
    public static class Static {
        #region GDSCRIPT INTEROP UTILITIES

        public static bool IsInstanceOfGDScript(Godot.Object godotObject, Godot.Script script) {
            var instanceScript = (Godot.Script) godotObject.GetScript();
            while (instanceScript != null) {
                if (instanceScript == script)
                    return true;
                instanceScript = instanceScript.GetBaseScript();
            }
            return false;
        }
        #endregion
        #region  AUTOLOAD UTILITES

        public static Godot.Node ISISGlobals(this Godot.Node node) => node.GetNode("/root/Globals");
        public static Godot.Node DebugDraw(this Godot.Node node) => node.GetNode("/root/DebugDraw");

        #endregion
        #region  FLOAT UTILITES

        public static bool IsZero(this Real value) => value.EqualsF(0);
        public static bool EqualsF(this Real value, Real other) =>
            Mathf.IsEqualApprox(value, other);
        public static Real Sign(this Real value) => Mathf.Sign(value);
        public static Real Abs(this Real value) => Mathf.Abs(value);

        public const Real DegreeToRadian = 57.29578f;
        public const Real RadianToDegree = 1.0f / DegreeToRadian;
        public static Real DeltaAngleDegrees(Real a, Real b) {
            var speaA = SmallestPositveEquivalentAngleDegree(a);
            var speaB = SmallestPositveEquivalentAngleDegree(b);
            var result = Mathf.Abs(speaA - speaB);
            return result > 180f ? result - 360f : result;
        }

        public static Real SmallestPositveEquivalentAngleDegree(Real angle) {
            angle %= 360f;
            if (angle < 0f)
                return angle + 360f;
            return angle;
        }

        public static Real DeltaAngleRadians(Real a, Real b) {
            var speaA = SmallestPositveEquivalentAngleRadians(a);
            var speaB = SmallestPositveEquivalentAngleRadians(b);
            var result = Mathf.Abs(speaA - speaB);
            return result > Mathf.Pi ? result - Mathf.Pi : result;
        }

        public static Real SmallestPositveEquivalentAngleRadians(Real angle) {
            angle %= Mathf.Tau;
            if (angle < 0f)
                return angle + Mathf.Tau;
            return angle;
        }

        public static Real SmallestEquivalentAngleRadians(Real angle) {
            angle %= Mathf.Tau;
            if (angle > Mathf.Pi)
                angle -= Mathf.Tau;
            else if (angle < -Mathf.Pi)
                angle += Mathf.Tau;
            return angle;
        }

        public static Real SmallestEquivalentAngleDegree(Real angle) {
            angle %= Mathf.Tau;
            if (angle > Mathf.Pi)
                angle -= Mathf.Tau;
            else if (angle < -Mathf.Pi)
                angle += Mathf.Tau;
            return angle;
        }

        #endregion
        #region  VECTOR3 UTILITIES

        public static bool IsZero(this Godot.Vector3 value) =>
            value == Vector3.Zero;

        public static Godot.Vector3 Min(this Godot.Vector3 value, Godot.Vector3 other) =>
            value < other ? value : other;

        public static Vector3 Max(this Vector3 value, Vector3 other) =>
            value > other ? value : other;

        public static Vector3 ClampVector(this Vector3 value,
                Vector3 min,
                Vector3 max) =>
            value.Max(min).Min(max);

        public static Vector3 ClampVectorComponents(this Vector3 value,
                Vector3 min,
                Vector3 max) =>
            new Godot.Vector3(
                Mathf.Clamp(value.x, min.x, max.x),
                Mathf.Clamp(value.y, min.y, max.y),
                Mathf.Clamp(value.z, min.z, max.z)
            );
        public static Vector3 ClampVectorComponents(this Vector3 value, Vector3 clamp) =>
            new Vector3(
                Mathf.Clamp(value.x, -clamp.x, clamp.x),
                Mathf.Clamp(value.y, -clamp.y, clamp.y),
                Mathf.Clamp(value.z, -clamp.z, clamp.z)
            );

        #endregion
        #region SPATIAL NODE UTILITIES

        public static Basis BasisFacingDirection(Vector3 forward) => BasisFacingDirection(forward, Vector3.Up);

        public static Basis BasisFacingDirection(Vector3 forward, Vector3 up) {
            var z = forward.Normalized();
            var x = up.Cross(z).Normalized();
            var y = z.Cross(x).Normalized();
            return new Basis(x, y, z);
        }

        public static Vector3 TransformPoint(this Transform transform, Vector3 point) => transform.basis.Xform(point) + transform.origin;
        public static Vector3 TransformPointInv(this Transform transform, Vector3 point) => transform.basis.XformInv(point) - transform.origin;
        public static Vector3 TransformVector(this Transform transform, Vector3 vector) => transform.basis.Xform(vector);
        public static Vector3 TransformVectorInv(this Transform transform, Vector3 vector) => transform.basis.XformInv(vector);
        public static Vector3 TransformDirection(this Transform transform, Vector3 direction) => transform.basis.Orthonormalized().Xform(direction);
        public static Vector3 TransformDirectionInv(this Transform transform, Vector3 direction) => transform.basis.Orthonormalized().XformInv(direction);

        public static Vector3 GlobalTranslation(this Godot.Spatial self) => self.GlobalTransform.origin;

        #endregion
        #region  RELATIVE DIRECTION UTILITIES

        public enum GeneralRelativeDirection {
            Ahead,
            Aside,
            Behind
        }

        public const Real DirectionDeterminationCosThreshold = 0.707f;

        public static GeneralRelativeDirection GetGeneralRelativeDirectionOfTransforms(
            Transform currentTransform,
            Transform targetTransform,
            Real cosThreshold = DirectionDeterminationCosThreshold
        ) => GetGeneralRelativeDirectionOfPositions(
            targetTransform.origin,
            currentTransform.origin,
            currentTransform.basis.z,
            cosThreshold);

        public static GeneralRelativeDirection GetGeneralRelativeDirectionOfPositions(
            Vector3 position,
            Vector3 currentPostion,
            Vector3 forwardDirection,
            Real cosThreshold = DirectionDeterminationCosThreshold
        ) {
            var targetDirection = (position - currentPostion).Normalized();
            return GetGeneralRelativeDirectionOfDirections(targetDirection, forwardDirection, cosThreshold);
        }

        public static GeneralRelativeDirection GetGeneralRelativeDirectionOfDirections(
            Vector3 direction,
            Vector3 forwardDirection,
            Real cosThreshold = DirectionDeterminationCosThreshold
        ) {
            var forwardness = forwardDirection.Dot(direction);
            if (forwardness > cosThreshold) {
                return GeneralRelativeDirection.Ahead;
            } else if (forwardness < -cosThreshold) {
                return GeneralRelativeDirection.Behind;
            } else {
                return GeneralRelativeDirection.Aside;
            }
        }
        #endregion
        #region PHYSICS UTILITIES

        #endregion
        #region STEERING BEHAVIOR UTILITIES
        public static(Vector3 linearInput, Vector3 angularInput) Add(this(Vector3, Vector3) value, (Vector3, Vector3) other) =>
            (value.Item1 + other.Item1, value.Item1 + other.Item2);

        public static(Vector3 linearInput, Vector3 angularInput) Scale(this(Vector3, Vector3) value, Real scalar) =>
            (value.Item1 * scalar, value.Item1 * scalar);

        #endregion
    }
}