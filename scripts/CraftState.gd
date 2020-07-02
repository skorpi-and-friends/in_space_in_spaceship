extends Resource

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

# Speed to travel at when there is no input i.e. how fast to travel when idle.
# In m/s.
export var set_speed: Vector3


# Forward dampener state. Whether or not to respect linear_v_limit in the z axis.
export var forward_dampener_on: bool = true

# Starfe dampener state. Whether or not to respect linear_v_limit in in the X or Y axis.
export var starfe_dampener_on: bool = true

# Angular dampener state. Whether or not to respect angular_v_limit.
export var angular_dampener_on: bool = true

# Acceleration dampener state. Whether or not to respect acceleration_limit.
export var acceleration_dampener_on: bool = true

# Total mass of the craft.
# In KG.
export var mass:float = 15000

# Acceleration limit
# In m/s.
export var acceleration_limit: Vector3 = Vector3(999, 999, 999)

# Linear velocity limit. To be respected no matter the linear input.
# In m/s.
export var linear_v_limit: Vector3 = Vector3(40, 40, 100)

# Angular velocity limit. To be respected no matter the linear input.
# In rad/s.
export var angular_v_limit: Vector3 = Vector3(2, 2, 2)

# Max force the linear thrusters are capable of exerting.
# In Newtons.
export var linear_thruster_force:Vector3 = Vector3(1_000_000, 1_000_000, 1_500_000)

# Max torque the angular thrusters are capable of exerting.
# In Newtons.
export var angular_thruster_force: Vector3 = Vector3(1_000_000, 1_000_000, 1_000_000)
