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

    /// <summary>
    /// Use this to convert <see cref="SteeringRoutineClosure"/>s to a <see cref="ISteeringRoutine"/>
    /// implementing struct. Both are forms of the Strategy Pattern.
    /// </summary>
    public struct SteeringRoutineClosureAdapter : ISteeringRoutine {
        public SteeringRoutineClosure Closure { get; set; }

        public SteeringRoutineClosureAdapter(SteeringRoutineClosure closure) {
            Closure = closure;
        }

        public SteeringInput CalculateSteering(Transform currentTransform, CraftStateWrapper currentState) =>
            new SteeringInput(Closure.Invoke(currentTransform, currentState));
    }

    #endregion

}