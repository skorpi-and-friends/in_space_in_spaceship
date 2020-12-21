extends Node

class_name CraftState

# Linear velocity in local-space.
# In m/s.
export var linear_velocity: Vector3

# Angular velocity in local-space.
# In rad/s.
export var angular_velocity: Vector3

# Input vector for a druver to work on. Meaning depends on implementation.
# Usually represent wanted Velocity.
export var linear_input: Vector3
export var angular_input: Vector3


# Vector output by a driver and used by a motor. Meaning depends on implementation.
# Usually represent represents force to apply to the rigidbody.
export var linear_flame: Vector3
export var angular_flame: Vector3

# Angular thruster toruqe, transient auto cacluated value from the angular_thrustuer_force
# according to the craft's shape and mass.
# In  Newton meters.
export var angular_thruster_torque: Vector3

# Moment of inertia, transient auto cacluated value used to convert the required angular 
# acceleration into the appropriate torque. Aquried directly from Godot's physics engine.
# In  kg*m*m.
# Default to one to avoid hard to track division by zero errors. The moi is asychronously
# retrieved from the engine and some frames pass before it happens. Time enough for the NANs
# to propagate EVERYWHERE!
export var moment_of_inertia := Vector3.ONE;

# angular acceleration limit, another transient auto cacluated value. It's cacluated from
# the normal acceleration limit (which is in m/ss) and adjusted to the size/shape of the craft.
# In rad/s/s.
export var angular_acceleration_limit: Vector3

# Speed to travel at when there is no input i.e. how fast to travel when idle.
# In m/s.
export var set_speed: Vector3

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
export var mass:float = 15000

# Acceleration limit
# In m/s.
export var acceleration_limit: Vector3 = Vector3(6, 6, 6)

# Number by which to multiply the acceleration limit.
export var acceleration_multiplier:float = 9.81

# Linear velocity limit. To be respected no matter the linear input.
# In m/s.
export var linear_v_limit: Vector3 = Vector3(40, 40, 100)

# Angular velocity limit. To be respected no matter the linear input.
# In rad/s.
export var angular_v_limit: Vector3 = Vector3(2, 2, 2)

# Number by which to multiply the thruster forces.
export var force_multiplier:float = 1_000_000

# Max force the linear thrusters are capable of exerting.
# In Newtons.
export var linear_thruster_force:Vector3 = Vector3(1, 1, 1.5)

# Max force the angular thrusters are capable of exerting.
# In Newtons.
export var angular_thruster_force: Vector3 = Vector3(1, 1, 1)


func _init_from_config(config: CraftConfig):
	set_speed = config.set_speed;
	limit_forward_v = config.limit_forward_v;
	limit_strafe_v = config.limit_strafe_v;
	limit_angular_v = config.limit_angular_v;
	limit_acceleration = config.limit_acceleration;
	acceleration_limit = config.acceleration_limit;
	linear_v_limit = config.linear_v_limit;
	angular_v_limit = config.angular_v_limit;
	force_multiplier = config.force_multiplier;
	linear_thruster_force = config.linear_thruster_force;
	angular_thruster_force = config.angular_thruster_force;
	mass = config.mass;
	acceleration_multiplier = config.acceleration_multiplier;
