using Godot;

#if GODOT_Real_IS_DOUBLE
using Real = System.Double;
#else
using Real = System.Single;
#endif

namespace ISIS.Minds.SteeringBehaviors {
    // TODO: Decide on either an OOP approach or the closure approach.
    // TODO: consider a generator approach 

    /// <summary>
    ///     A function that's to be polled every frame (or so) to calculate the steering action the
    ///     craft is supposed to take.
    /// </summary>
    /// <returns>
    ///     <para> LinearInput as a percentage LinearVLimit with respect to global rotation.</para>
    ///     <para>AngularInput as velocity to be maintained.</para>
    /// </returns>
    public delegate(Vector3 linearInput, Vector3 angularInput) SteeringRoutineClosure(Transform currentTransform,
        CraftStateWrapper currentState);

    public interface ISteeringRoutine {
        SteeringInput CalculateSteering(Transform currentTransform, CraftStateWrapper currentState);
    }
}