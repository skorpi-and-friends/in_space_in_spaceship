using Godot;

#if GODOT_Real_IS_DOUBLE
using Real = System.Double;
#else
using Real = System.Single;
#endif

namespace ISIS.Minds.SteeringBehaviors {
    /// <summary>
    ///     A function that's to be polled every frame (or so) to calculate the angular input a craft is supposed to take.
    /// </summary>
    /// <returns>Returns angular input as velocity to be maintained.</returns>
    public delegate Vector3 AngularRoutineClosure(Transform currentTransform,
        CraftStateWrapper currentState);

    public interface IAngularRoutine {
        Vector3 CalculateSteering(Transform currentTransform, CraftStateWrapper currentState);
    }
}