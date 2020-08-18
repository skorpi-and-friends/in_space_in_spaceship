using Godot;

namespace ISIS {

    public static class Static {

        // ### GDSCRIPT INTEROP UTILITIES

        public static bool IsInstanceOfGDScript(Godot.Object godotObject, Godot.Script script) {
            var instanceScript = (Godot.Script) godotObject.GetScript();
            while (instanceScript != null) {
                if (instanceScript == script)
                    return true;
                instanceScript = instanceScript.GetBaseScript();
            }
            return false;
        }

        // ### AUTOLOAD UTILITES

        public static Godot.Node ISISGlobals(this Godot.Node node) => node.GetNode("/root/Globals");

        // ### FLOAT UTILITES

        public static float Sign(this float value) => Mathf.Sign(value);
        public static float Abs(this float value) => Mathf.Abs(value);

        public const float DegreeToRadian = 57.29578f;
        public const float RadianToDegree = 1.0f / DegreeToRadian;
        public static float DeltaAngleDegrees(float a, float b) {
            var speaA = SmallestPositveEquivalentAngleDegree(a);
            var speaB = SmallestPositveEquivalentAngleDegree(b);
            var result = Mathf.Abs(speaA - speaB);
            return result > 180f ? result - 360f : result;
        }

        public static float SmallestPositveEquivalentAngleDegree(float angle) {
            angle = angle % 360f;
            if (angle < 0f)
                return angle + 360f;
            return angle;
        }

        public static float DeltaAngleRadians(float a, float b) {
            var speaA = SmallestPositveEquivalentAngleRadians(a);
            var speaB = SmallestPositveEquivalentAngleRadians(b);
            var result = Mathf.Abs(speaA - speaB);
            return result > Mathf.Pi ? result - Mathf.Pi : result;
        }

        public static float SmallestPositveEquivalentAngleRadians(float angle) {
            angle = angle % Mathf.Tau;
            if (angle < 0f)
                return angle + Mathf.Tau;
            return angle;
        }

        public static float SmallestEquivalentAngleRadians(float angle) {
            angle = angle % Mathf.Tau;
            if (angle > Mathf.Pi)
                angle -= Mathf.Tau;
            else if (angle < -Mathf.Pi)
                angle += Mathf.Tau;
            return angle;
        }

        public static float SmallestEquivalentAngleDegree(float angle) {
            angle = angle % Mathf.Tau;
            if (angle > Mathf.Pi)
                angle -= Mathf.Tau;
            else if (angle < -Mathf.Pi)
                angle += Mathf.Tau;
            return angle;
        }

        // ### VECTOR3 UTILITIES

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

        // ### SPATIAL NODE UTILITIES

        public static Basis BasisFacingDirection(Vector3 forward) => BasisFacingDirection(forward, Vector3.Up);

        public static Basis BasisFacingDirection(Vector3 forward, Vector3 up) {
            var z = forward.Normalized();
            var x = up.Cross(z).Normalized();
            var y = z.Cross(x).Normalized();
            return new Basis(x, y, z);
        }

        public static Vector3 TransformPoint(this Transform transform, Vector3 point) => transform.basis.Xform(point) + transform.origin;
        public static Vector3 TransformPointInv(this Transform transform, Vector3 point) => transform.basis.XformInv(point) + transform.origin;
        public static Vector3 TransformVector(this Transform transform, Vector3 vector) => transform.basis.Xform(vector);
        public static Vector3 TransformVectorInv(this Transform transform, Vector3 vector) => transform.basis.XformInv(vector);
        public static Vector3 TransformDirection(this Transform transform, Vector3 direction) => transform.basis.Orthonormalized().Xform(direction);
        public static Vector3 TransformDirectionInv(this Transform transform, Vector3 direction) => transform.basis.Orthonormalized().XformInv(direction);

        public static Vector3 GlobalTranslation(this Godot.Spatial self) => self.GlobalTransform.origin;

        // ### RELATIVE DIRECTION UTILITIES

        public enum GeneralRelativeDirection {
            Ahead,
            Aside,
            Behind
        }

        public const float DirectionDeterminationCosThreshold = 0.707f;

        public static GeneralRelativeDirection GetGeneralRelativeDirectionOfTransforms(
            Transform currentTransform,
            Transform targetTransform,
            float cosThreshold = DirectionDeterminationCosThreshold
        ) => GetGeneralRelativeDirectionOfPositions(
            targetTransform.origin,
            currentTransform.origin,
            currentTransform.basis.z,
            cosThreshold);

        public static GeneralRelativeDirection GetGeneralRelativeDirectionOfPositions(
            Vector3 position,
            Vector3 currentPostion,
            Vector3 forwardDirection,
            float cosThreshold = DirectionDeterminationCosThreshold
        ) {
            var targetDirection = (position - currentPostion).Normalized();
            return GetGeneralRelativeDirectionOfDirections(targetDirection, forwardDirection, cosThreshold);
        }

        public static GeneralRelativeDirection GetGeneralRelativeDirectionOfDirections(
            Vector3 direction,
            Vector3 forwardDirection,
            float cosThreshold = DirectionDeterminationCosThreshold
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
    }

}