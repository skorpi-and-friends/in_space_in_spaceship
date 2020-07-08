extends Resource

# export var state: Resource
# export var linear_pid: Resource
# export var angular_pid: Resource
class_name CraftConfig

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
export var angular_thruster_force: Vector3 = Vector3(1, 1, 1.5)

export var linear_input_multiplier := 1.0;
export var angular_input_multiplier := 1.0;

export var linear_integrat_max: float;
export var linear_integrat_min: float;
export var linear_proportional_gain: float;
export var linear_integrat_gain: float;
export var linear_differential_gain: float;

export var angular_integrat_max: float;
export var angular_integrat_min: float;
export var angular_proportional_gain: float;
export var angular_integrat_gain: float;
export var angular_differential_gain: float;