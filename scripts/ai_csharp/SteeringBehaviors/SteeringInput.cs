using Godot;
using static ISIS.Static;

#if GODOT_Real_IS_DOUBLE
using Real = System.Double;
#else
using Real = System.Single;
#endif

namespace ISIS.SteeringBehaviors {
    public struct SteeringInput {
        public Vector3 LinearInput { get; set; }
        public Vector3 AngularInput { get; set; }
        public SteeringInput(Vector3 linearInput, Vector3 angularInput) {
            LinearInput = linearInput;
            AngularInput = angularInput;
        }

        public SteeringInput((Vector3, Vector3) input) {
            LinearInput = input.Item1;
            AngularInput = input.Item2;
        }

        public static SteeringInput Zero => new SteeringInput(Vector3.Zero, Vector3.Zero);

        public void Deconstruct(out Vector3 linearInput, out Vector3 angularInput) {
            linearInput = LinearInput;
            angularInput = AngularInput;
        }
    }
}