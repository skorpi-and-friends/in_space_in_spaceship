using System;
using Godot;

#if GODOT_Real_IS_DOUBLE
using Real = System.Double;
#else
using Real = System.Single;
#endif
namespace ISIS {
    public struct CraftStateWrapper {
        private readonly Godot.Object _craftStateActual;

        public CraftStateWrapper(Godot.Object actualCraftState) {
            _craftStateActual = actualCraftState;
        }

        /// <summary>
        /// Linear velocity in local-space.
        /// In m/s.
        /// </summary>
        public Vector3 LinearVelocty {
            get => (Vector3) _craftStateActual.Get("linear_velocity");
            set => _craftStateActual.Set("linear_velocity", value);
        }

        /// <summary>
        /// Angular velocity in local-space.
        /// In rad/s.
        /// </summary>
        public Vector3 AngularVelocty {
            get => (Vector3) _craftStateActual.Get("angular_velocity");
            set => _craftStateActual.Set("angular_velocity", value);
        }

        /// <summary>
        /// Input vector for a druver to work on. Meaning depends on implementation.
        /// Usually represent wanted Velocity.
        /// </summary>
        public Vector3 LinearInput {
            get => (Vector3) _craftStateActual.Get("linear_input");
            set => _craftStateActual.Set("linear_input", value);
        }

        public Vector3 AngularInput {
            get => (Vector3) _craftStateActual.Get("angular_input");
            set => _craftStateActual.Set("angular_input", value);
        }

        /// <summary>
        /// Vector output by a driver and used by a motor. Meaning depends on implementation.
        /// Usually represent represents force to apply to the rigidbody.
        /// </summary>
        public Vector3 LinearFlame {
            get => (Vector3) _craftStateActual.Get("linear_flame");
            set => _craftStateActual.Set("linear_flame", value);
        }
        public Vector3 AngularFlame {
            get => (Vector3) _craftStateActual.Get("angular_flame");
            set => _craftStateActual.Set("angular_flame", value);
        }

        /// <summary>
        /// Angular thruster toruqe, transient auto cacluated value from the angular_thrustuer_force
        /// according to the craft's shape and mass.
        /// In  Newton meters.
        /// </summary>
        public Vector3 AngularThrusterTorque {
            get => (Vector3) _craftStateActual.Get("angular_thruster_torque");
            set => _craftStateActual.Set("angular_thruster_torque", value);
        }

        /// <summary>
        /// Moment of inertia, transient auto cacluated value used to convert the required angular
        /// acceleration into the appropriate torque. Aquried directly from Godot's physics engine.
        /// In  kg*m*m.
        /// </summary>
        public Vector3 MomentOfInertia {
            get => (Vector3) _craftStateActual.Get("moment_of_inertia");
            set => _craftStateActual.Set("moment_of_inertia", value);
        }

        /// <summary>
        /// angular acceleration limit, another transient auto cacluated value. It's cacluated from
        /// the normal acceleration limit (which is in m/ss) and adjusted to the size/shape of the craft.
        /// In rad/s/s.
        /// </summary>
        public Vector3 AngularAccelerationLimit {
            get => (Vector3) _craftStateActual.Get("angular_acceleration_limit");
            set => _craftStateActual.Set("angular_acceleration_limit", value);
        }

        /// <summary>
        /// Speed to travel at when there is no input i.e. how fast to travel when idle.
        /// In m/s.
        /// </summary>
        public Vector3 SetSpeed {
            get => (Vector3) _craftStateActual.Get("set_speed");
            set => _craftStateActual.Set("set_speed", value);
        }

        /// <summary>
        /// Whether or not to respect linear_v_limit in the z axis.
        /// </summary>
        public bool LimitForwardV {
            get => (bool) _craftStateActual.Get("limit_forward_v");
            set => _craftStateActual.Set("limit_forward_v", value);
        }

        /// <summary>
        /// Whether or not to respect linear_v_limit in the X or Y axis.
        /// </summary>
        public bool LimitStrafeV {
            get => (bool) _craftStateActual.Get("limit_strafe_v");
            set => _craftStateActual.Set("limit_strafe_v", value);
        }

        /// <summary>
        /// Whether or not to respect angular_v_limit.
        /// </summary>
        public bool LimitAngularV {
            get => (bool) _craftStateActual.Get("limit_angular_v");
            set => _craftStateActual.Set("limit_angular_v", value);
        }

        /// <summary>
        /// Whether or not to respect acceleration_limit.
        /// </summary>
        public bool LimitAcceleration {
            get => (bool) _craftStateActual.Get("limit_acceleration");
            set => _craftStateActual.Set("limit_acceleration", value);
        }

        /// <summary>
        /// Total mass of the craft.
        /// In KG.
        /// </summary>
        public Real Mass {
            get => (Real) _craftStateActual.Get("mass");
            set => _craftStateActual.Set("mass", value);
        }

        /// <summary>
        /// Acceleration limit
        /// In m/s.
        /// </summary>
        public Vector3 AccelerationLimit {
            get => (Vector3) _craftStateActual.Get("acceleration_limit");
            set => _craftStateActual.Set("acceleration_limit", value);
        }

        /// <summary>
        /// Number by which to multiply the acceleration limit.
        /// </summary>
        public Real AccelerationMultiplier {
            get => (Real) _craftStateActual.Get("acceleration_multiplier");
            set => _craftStateActual.Set("acceleration_multiplier", value);
        }

        /// <summary>
        /// Linear velocity limit. To be respected no matter the linear input.
        /// In m/s.
        /// </summary>
        public Vector3 LinearVLimit {
            get => (Vector3) _craftStateActual.Get("linear_v_limit");
            set => _craftStateActual.Set("linear_v_limit", value);
        }

        /// <summary>
        /// Angular velocity limit. To be respected no matter the linear input.
        /// In rad/s.
        /// </summary>
        public Vector3 AngularVLimit {
            get => (Vector3) _craftStateActual.Get("angular_v_limit");
            set => _craftStateActual.Set("angular_v_limit", value);
        }

        /// <summary>
        /// Number by which to multiply the thruster forces.
        /// </summary>
        public Real ForceMultiplier {
            get => (Real) _craftStateActual.Get("force_multiplier");
            set => _craftStateActual.Set("force_multiplier", value);
        }

        /// <summary>
        /// Max force the linear thrusters are capable of exerting.
        /// In Newtons.
        /// </summary>
        public Vector3 LinearThrusterForce {
            get => (Vector3) _craftStateActual.Get("linear_thruster_force");
            set => _craftStateActual.Set("linear_thruster_force", value);
        }

        /// <summary>
        /// Max force the angular thrusters are capable of exerting.
        /// In Newtons.
        /// </summary>
        public Vector3 AngularThrusterForce {
            get => (Vector3) _craftStateActual.Get("angular_thruster_force");
            set => _craftStateActual.Set("angular_thruster_force", value);
        }

        public void SetCraftInput(Vector3 linearInput, Vector3 angularInput) {
            LinearInput = linearInput;
            AngularInput = angularInput;
        }
    }
}