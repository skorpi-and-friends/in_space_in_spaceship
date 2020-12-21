using System.Diagnostics;
using Godot;
using GreenBehaviors;
using GreenBehaviors.Composite;
using GreenBehaviors.Decorator;
using GreenBehaviors.LeafLambda;
using static ISIS.Static;

#if GODOT_Real_IS_DOUBLE
using Real = System.Double;
#else
using Real = System.Single;
#endif

namespace ISIS.Minds.SteeringBehaviors {

    #region OOP HELPERS

    #endregion
    /// <summary>
    /// Use this to convert <see cref="LinearRoutineClosure"/>s to a <see cref="ILinearRoutine"/>
    /// implementing struct. Both are forms of the Strategy Pattern.
    /// </summary>
    public struct LinearRoutineClosureAdapter : ILinearRoutine {
        public LinearRoutineClosure Closure { get; set; }

        public LinearRoutineClosureAdapter(LinearRoutineClosure closure) {
            Closure = closure;
        }

        public Vector3 CalculateSteering(Transform currentTransform, CraftStateWrapper currentState) =>
            Closure.Invoke(currentTransform, currentState);
    }

}