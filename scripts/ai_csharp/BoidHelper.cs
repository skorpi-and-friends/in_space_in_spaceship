using System.Collections;
using System.Collections.Generic;
using Godot;

#if GODOT_Real_IS_DOUBLE
using Real = System.Double;
#else
using Real = System.Single;
#endif

namespace ISIS {
    public static class BoidHelper {
        public const int DirectionCount = 300;
        public static readonly Vector3[] directions;

        static BoidHelper() {
            directions = new Vector3[BoidHelper.DirectionCount];

            Real goldenRatio = (1 + Mathf.Sqrt(5)) / 2;
            Real angleIncrement = Mathf.Pi * 2 * goldenRatio;

            for (int i = 0; i < DirectionCount; i++) {
                Real t = (Real) i / DirectionCount;
                Real inclination = Mathf.Acos(1 - (2 * t));
                Real azimuth = angleIncrement * i;

                Real x = Mathf.Sin(inclination) * Mathf.Cos(azimuth);
                Real y = Mathf.Sin(inclination) * Mathf.Sin(azimuth);
                Real z = Mathf.Cos(inclination);
                directions[i] = new Vector3(x, y, z).Normalized();
            }
        }
    }
}