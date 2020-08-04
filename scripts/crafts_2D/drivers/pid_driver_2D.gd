extends CraftDriver2D

class_name PIDDriver2D

onready var _linear_pid := $LinearPID as PIDControllerVector2D;
onready var _angular_pid := $AngularPID as PIDController;


func _update_flames(state: CraftState2D):
	state.linear_input *= _linear_input_multiplier;
	state.angular_input *= _angular_input_multiplier;
	update_linear_flames(state);
	update_angular_flames(state);


func update_linear_flames(state: CraftState2D):
	var pid := _linear_pid;
	
	# capture the velocity
	var linear_input := state.linear_input;
	
	var is_moving_forward := linear_input.y > 0;

	var v_limit := state.linear_v_limit;

	if state.limit_strafe_v:
		# if dampeners are on, clamp the input to the limit
		linear_input = Utility.clamp_vector_components_2D(
				linear_input, -v_limit, v_limit);
	
	# if forward dampener is off, 
	if !state.limit_forward_v:
		# restore the clamped input by the z
		linear_input.y = state.linear_input.y;

	var max_force := state.linear_thruster_force * state.force_multiplier;

	# use strafe thrusters if moving backwards
	max_force.y = max_force.y if is_moving_forward else max_force.x; 

	# calculate the max acceleration the available force allows us
	var acceleration_limit := max_force / state.mass;

	if state.limit_acceleration:
		var manual_acceleration_limit := state.acceleration_limit * state.acceleration_multiplier;
		# clamp the acceleration according to the limit
		acceleration_limit = Utility.clamp_vector_components_2D(
				acceleration_limit,
				-manual_acceleration_limit,
				manual_acceleration_limit);
	
	# step the driver to calcuate the flame
	var linear_flame_vector := pid.update(
		state.linear_velocity,
		linear_input - state.linear_velocity, # error
		1
	);
	
	linear_flame_vector = Utility.clamp_vector_components_2D(
			linear_flame_vector,
			-acceleration_limit, # min flame
			acceleration_limit); # max flame
	
	state.linear_flame = linear_flame_vector; #* state.Mass;


func update_angular_flames(state: CraftState2D):
	var pid := _angular_pid;
	# look at LinearFlamesUpdate for comments
	
	var angular_input := state.angular_input;
	
	if state.limit_angular_v:
		angular_input = clamp(
				angular_input,
				-state.angular_v_limit, 
				state.angular_v_limit);
	
	var max_torque := state.angular_thruster_torque * state.force_multiplier;

	# difference here in how we calculate max available acceleration allows to us
	var acceleration_limit := max_torque / state.moment_of_inertia;

	if state.limit_acceleration:
		# no need to use the acceleration multiplier: it's already
		# been used when converting from the linear_accel_limet 
		var manual_acceleration_limit := state.angular_acceleration_limit;
		# clamp the acceleration according to the limit
		acceleration_limit = clamp(
				acceleration_limit,
				-manual_acceleration_limit,
				manual_acceleration_limit);
	
	# step the driver to calcuate the flame
	var angular_flame_vector := pid.update(
		state.angular_velocity,
		angular_input - state.angular_velocity, # error
		1
	);
	
	angular_flame_vector = clamp(
			angular_flame_vector,
			-acceleration_limit, # min flame
			acceleration_limit); # max flame
	
	state.angular_flame = angular_flame_vector; #* state.Mass;

