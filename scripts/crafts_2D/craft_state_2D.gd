extends Node

class_name CraftState2D
# Linear velocity in local-space.
# In m/s.
export var linear_velocity: Vector2

# Angular velocity in local-space.
# In rad/s.
export var angular_velocity: float

# Input vector for a driver to work on. Meaning depends on implementation.
# Usually represent wanted Velocity.
export var linear_input: Vector2
export var angular_input: float


# Vector output by a driver and used by a motor. Meaning depends on implementation.
# Usually represent represents force to apply to the rigidbody.
export var linear_flame: Vector2
export var angular_flame: float

# Angular thruster toruqe, transient auto cacluated value from the angular_thrustuer_force
# according to the craft's shape and mass.
# In  Newton meters.
export var angular_thruster_torque: float

# Moment of inertia, transient auto cacluated value used to convert the required angular 
# acceleration into the appropriate torque. Aquried directly from Godot's physics engine.
# In  kg*m*m.
export var moment_of_inertia := 1.0;

# angular acceleration limit, another transient auto cacluated value. It's cacluated from
# the normal acceleration limit (which is in m/ss) and adjusted to the size/shape of the craft.
# In rad/s/s.
export var angular_acceleration_limit: float

# Velocty to travel at when there is no input i.e. how fast to travel when idle.
# In m/s.
export var idle_velocity: Vector2

# Whether or not to respect linear_v_limit in the z axis.
export var limit_forward_v: bool = true

# Whether or not to respect linear_v_limit in in the X or Y axis.
export var limit_strafe_v: bool = true

# Whether or not to respect angular_v_limit.
export var limit_angular_v: bool = true

# Whether or not to respect acceleration_limit.
export var limit_acceleration: bool = true

# Total mass of the craft.
# In KG.
export var mass:float = 15_000.0

# Acceleration limit
# In m/s.
export var acceleration_limit: Vector2 = Vector2(6, 6)

# Number by which to multiply the acceleration limit.
export var acceleration_multiplier:float = 9.81

# Linear velocity limit. To be respected no matter the linear input.
# In m/s.
export var linear_v_limit: Vector2 = Vector2(40, 100)

# Angular velocity limit. To be respected no matter the linear input.
# In rad/s.
export var angular_v_limit: float = 2

# Number by which to multiply the thruster forces.
export var force_multiplier:float = 1_000_000

# Max force the linear thrusters are capable of exerting.
# In Newtons.
export var linear_thruster_force:Vector2 = Vector2(1, 1.5)

# Max force the angular thrusters are capable of exerting.
# In Newtons.
export var angular_thruster_force: float = 1.0

