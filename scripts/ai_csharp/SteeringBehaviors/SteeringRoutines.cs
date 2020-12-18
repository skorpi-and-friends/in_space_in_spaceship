using Godot;
using static ISIS.Static;

#if GODOT_Real_IS_DOUBLE
using Real = System.Double;
#else
using Real = System.Single;
#endif

namespace ISIS.Minds.SteeringBehaviors {
    public static partial class SteeringRoutines {
        public static Vector3 FacePositionAngularInput(Vector3 position, Transform currentTransform) =>
            FaceLocalDirectionAngularInput(currentTransform.basis.XformInv(position - currentTransform.origin));
        public static Vector3 FaceDirectionAngularInput(Vector3 direction, Transform currentTransform) =>
            FaceLocalDirectionAngularInput(currentTransform.basis.XformInv(direction));

        // FIXME: improve this
        // make the return value a fraction of angular_v_limit
        public static Vector3 FaceLocalDirectionAngularInput(Vector3 direction) {
            var temp = BasisFacingDirection(direction).GetEuler();
            return new Vector3(
                temp.x.Sign() * DeltaAngleRadians(0f, temp.x).Abs(),
                temp.y.Sign() * DeltaAngleRadians(0f, temp.y).Abs(),
                temp.z.Sign() * DeltaAngleDegrees(0f, temp.z).Abs()
            );
        }

        #region OOP HELPERS
        /// <summary>
        /// Use this to convert <see cref="ISteeringRoutine"/> implementing objects to the
        /// closure form of SteeringRoutines.
        /// </summary>
        public static SteeringRoutineClosure SteeringRoutineObjectAdapter(ISteeringRoutine routine) =>
            (Transform currentTransform, CraftStateWrapper currentState) => {
                var(linearInput, angularInput) = routine.CalculateSteering(currentTransform, currentState);
                return (linearInput, angularInput);
            };

        #endregion

        #region COMPOSABILITY
        public static SteeringRoutineClosure LinearAngularRoutineComposer(
            LinearRoutineClosure LinearRoutine,
            AngularRoutineClosure AngularRoutine
        ) {
            return (Transform currentTransform, CraftStateWrapper currentState) => {
                return (
                    LinearRoutine(currentTransform, currentState),
                    AngularRoutine(currentTransform, currentState)
                );
            };
        }

        public static SteeringRoutineClosure NoAngularInputComposer(
            LinearRoutineClosure linearRoutine
        ) {
            return (Transform currentTransform, CraftStateWrapper currentState) => {
                return (
                    linearRoutine(currentTransform, currentState),
                    Vector3.Zero
                );
            };
        }

        public static SteeringRoutineClosure FirstPriorityRoutineComposer(
            params SteeringRoutineClosure[] routines
        ) {
            var length = routines.Length;
            return (Transform currentTransform, CraftStateWrapper currentState) => {
                var linearInput = Vector3.Zero;
                var angularInput = Vector3.Zero;
                for (int ii = 0; ii < length; ii++) {
                    (linearInput, angularInput) = routines[ii].Invoke(currentTransform, currentState);
                    if (!linearInput.IsZero() || !angularInput.IsZero()) break;
                }
                return (linearInput, angularInput);
            };
        }

        public static LinearRoutineClosure FirstPriorityRoutineComposer(
            params LinearRoutineClosure[] routines
        ) {
            var length = routines.Length;
            return (Transform currentTransform, CraftStateWrapper currentState) => {
                var linearInput = Vector3.Zero;
                for (int ii = 0; ii < length; ii++) {
                    linearInput = routines[ii].Invoke(currentTransform, currentState);
                    if (!linearInput.IsZero()) break;
                }
                return linearInput;
            };
        }

        public static LinearRoutineClosure WeightedRoutineComposer(
            params(LinearRoutineClosure, Real) [] routinesWithWeights
        ) {
            var length = routinesWithWeights.Length;
            return (Transform currentTransform, CraftStateWrapper currentState) => {
                var linearInput = Vector3.Zero;
                for (int ii = 0; ii < length; ii++) {
                    linearInput = routinesWithWeights[ii].Item1.Invoke(currentTransform, currentState) * routinesWithWeights[ii].Item2;
                    if (!linearInput.IsZero()) break;
                }
                return linearInput;
            };
        }

        public static AngularRoutineClosure WeightedRoutineComposer(
            params(AngularRoutineClosure, Real) [] routinesWithWeights
        ) {
            var length = routinesWithWeights.Length;
            return (Transform currentTransform, CraftStateWrapper currentState) => {
                var linearInput = Vector3.Zero;
                for (int ii = 0; ii < length; ii++) {
                    linearInput = routinesWithWeights[ii].Item1.Invoke(currentTransform, currentState) * routinesWithWeights[ii].Item2;
                    if (!linearInput.IsZero()) break;
                }
                return linearInput;
            };
        }

        #endregion

        #region UTILITY

        public static AngularRoutineClosure LinearToAngularConverter(
            LinearRoutineClosure routine
        ) {
            return (Transform currentTransform, CraftStateWrapper currentState) =>
                FaceDirectionAngularInput(routine(currentTransform, currentState), currentTransform);
        }

        /// <summary>
        /// Get a set of routines orchestrated purely for survival.
        /// </summary>
        /// <seealso cref="SurvivalRoutineComposer"/>
        public static SteeringRoutineClosure GetSurvivalRoutine(
            RigidBody craft, Vector3 craftExtents, Real predectionTimeSeconds = 5,
            bool lookWhereYouGo = true
        ) {
            var linearRoutine = GetPreferredObstacleAvoidanceRoutine(craft, craftExtents, predectionTimeSeconds);
            return lookWhereYouGo ?
                LookWhereYouGoRoutineComposer(linearRoutine) : NoAngularInputComposer(linearRoutine);
        }

        /// <summary>
        /// Currently <see cref="AvoidObstacleSebLagueRay"/>
        /// </summary>
        public static LinearRoutineClosure GetPreferredObstacleAvoidanceRoutine(RigidBody craft, Vector3 craftExtents, Real predectionTimeSeconds = 5) {
            return AvoidObstacleSebLagueRay(craft, craftExtents, predectionTimeSeconds);
        }

        /// <summary>
        /// Composes with <see cref="AlignToVelocityAngularRoutine"/>
        /// </summary>
        /// <param name="alignToLinearInput">If true, will align along linear input as opposed to linear velocity</param>
        public static SteeringRoutineClosure LookWhereYouGoRoutineComposer(
            LinearRoutineClosure routine,
            bool alignToLinearInput = true
        ) {
            if (alignToLinearInput) {
                return (Transform currentTransform, CraftStateWrapper currentState) => {
                    var linearInput = routine(currentTransform, currentState);
                    return (
                        linearInput,
                        FaceDirectionAngularInput(linearInput.Normalized(), currentTransform)
                    );
                };
            } else {
                return (Transform currentTransform, CraftStateWrapper currentState) => {
                    return (
                        routine(currentTransform, currentState),
                        FaceDirectionAngularInput(currentState.LinearVelocty.Normalized(), currentTransform)
                    );
                };
            }
        }

        /// <summary>
        /// Wraps the given routine along with other <see cref="SteeringRoutine"/>s that are meant
        /// to ensure craft survival.
        /// </summary>
        /// <remarks>
        /// Currenlty, it uses <see cref="FirstPriorityRoutineComposer"/> to put the given routine after
        /// the preferred AvoidObstacle routine.
        /// The preferred AvoidObstacle looks where it goes.
        /// </remarks>
        /// <seealso cref="GetPreferredObstacleAvoidanceRoutine"/>
        public static SteeringRoutineClosure SurvivalRoutineComposer(
            SteeringRoutineClosure routine,
            RigidBody craft, Vector3 craftExtents, Real predectionTimeSeconds = 5
        ) {
            return FirstPriorityRoutineComposer(
                LookWhereYouGoRoutineComposer(
                    GetPreferredObstacleAvoidanceRoutine(craft, craftExtents, predectionTimeSeconds)
                ),
                routine
            );
        }

        public static LinearRoutineClosure SurvivalRoutineComposer(
            LinearRoutineClosure routine,
            RigidBody craft, Vector3 craftExtents, Real predectionTimeSeconds = 5
        ) {
            return FirstPriorityRoutineComposer(
                GetPreferredObstacleAvoidanceRoutine(craft, craftExtents, predectionTimeSeconds),
                routine
            );
        }
    }

    #endregion
}