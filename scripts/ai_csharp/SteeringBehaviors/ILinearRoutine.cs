using Godot;

#if GODOT_Real_IS_DOUBLE
using Real = System.Double;
#else
using Real = System.Single;
#endif

namespace ISIS.SteeringBehaviors {
    /// <summary>
    ///     A function that's to be polled every frame (or so) to calculate the linear input a craft is supposed to take.
    /// </summary>
    /// <returns>Returns linear input as a fraction LinearVLimit.</returns>
    public delegate Vector3 LinearRoutineClosure(Transform currentTransform,
        CraftStateWrapper currentState);

    public interface ILinearRoutine {
        Vector3 CalculateSteering(Transform currentTransform, CraftStateWrapper currentState);
    }
}