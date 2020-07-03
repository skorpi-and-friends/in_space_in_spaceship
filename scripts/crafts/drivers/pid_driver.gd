extends CraftDriver

var _linear_pid: PIDControllerVector;
var _angular_pid: PIDControllerVector;


func _init_from_config(config: CraftConfig):
	# call method on base class
	._init_from_config(config);
	
#	if $LinearPID:
#		_linear_pid = $LinearPID;
#	else:
#		_linear_pid = PIDControllerVector.new()
#		add_child(_linear_pid, false);
#
#	if $AngularPID:
#		_linear_pid = $AngularPID;
#	else:
#		_angular_pid = PIDControllerVector.new()
#		add_child(_angular_pid, false);

	# look for pid childern. Only usefull for viewing the pid state at runtime i.e.
	# we don't need to add new children if not found.
	_linear_pid = ($LinearPID as PIDControllerVector) if has_node("LinearPID") else PIDControllerVector.new();
	_angular_pid = ($AngularPID as PIDControllerVector) if has_node("AngularPID") else PIDControllerVector.new();
	
	# init linear pid state
	_linear_pid.integrat_max = Vector3.ONE * config.linear_integrat_max;
	_linear_pid.integrat_min = Vector3.ONE * config.linear_integrat_min;
	_linear_pid.proportional_gain = Vector3.ONE * config.linear_proportional_gain;
	_linear_pid.integrat_gain = Vector3.ONE * config.linear_integrat_gain;
	_linear_pid.differential_gain = Vector3.ONE * config.linear_differential_gain;
	
	# init angular pid
	_angular_pid.integrat_max = Vector3.ONE * config.angular_integrat_max;
	_angular_pid.integrat_min = Vector3.ONE * config.angular_integrat_min;
	_angular_pid.proportional_gain = Vector3.ONE * config.angular_proportional_gain;
	_angular_pid.integrat_gain = Vector3.ONE * config.angular_integrat_gain;
	_angular_pid.differential_gain = Vector3.ONE * config.angular_differential_gain;


func _update_flames(state: CraftState):
	state.linear_input *= _linear_input_multiplier;
	state.angular_flame *= _angular_input_multiplier;
	update_linear_flames(state);
	update_angular_flames(state);


func update_linear_flames(state: CraftState):
	var pid := _linear_pid;# as PIDControllerVector;
	
	# capture the velocity
	var linear_input := state.linear_input;
	
	var is_moving_forward := linear_input.z > 0;

	var v_limit := state.linear_v_limit;
	
	# if forward dampener is off, 
	if !state.forward_dampener_on:
		# eliminate V limit in the Z.
		v_limit.z = INF;

	if state.starfe_dampener_on:
		# if dampeners are on, clamp the input to the limit
		linear_input = Utility.clamp_vector_components(linear_input,-v_limit, v_limit);

	var max_force := state.linear_thruster_force * state.force_multiplier;

	# use strafe thrusters if moving backwards
	max_force.z = max_force.z if is_moving_forward else max_force.x; 

	# calculate the max acceleration the available force allows us
	var acceleration_limit := max_force / state.mass;

	if state.acceleration_dampener_on:
		# clamp the acceleration according to the limit
		acceleration_limit = Utility.clamp_vector_components(
				acceleration_limit,
				-state.acceleration_limit,
				state.acceleration_limit);
	
	# step the driver to calcuate the flame
	var linear_flame_vector := pid.update(
		linear_input,
		linear_input - state.linear_velocity, # error
		1
	);
	
	linear_flame_vector = Utility.clamp_vector_components(
			linear_flame_vector,
			-acceleration_limit, # min flame
			acceleration_limit); # max flame
	
	state.linear_flame = linear_flame_vector; #* state.Mass;


func update_angular_flames(state: CraftState):
	var pid := _angular_pid;# as PIDControllerVector;
	# look at LinearFlamesUpdate for comments
	
	var angular_input := state.angular_input;
	
	if angular_input:
		pass
	
	if state.angular_dampener_on:
		angular_input = Utility.clamp_vector_components(
				angular_input,
				-state.angular_v_limit, 
				state.angular_v_limit);
	
	var max_torque := state.angular_thruster_torque * state.force_multiplier;

	# difference here in how we calculate max available acceleration allows to us
	# TODO: manually caclulate inertia tensor
	var acceleration_limit := max_torque / state.mass;

	if state.acceleration_dampener_on:
		# clamp the acceleration according to the limit
		acceleration_limit = Utility.clamp_vector_components(
				acceleration_limit,
				-state.acceleration_limit,
				state.acceleration_limit);
	
	# step the driver to calcuate the flame
	var angular_flame_vector := pid.update(
		angular_input,
		angular_input - state.angular_velocity, # error
		1
	);
	
	angular_flame_vector = Utility.clamp_vector_components(
			angular_flame_vector,
			-acceleration_limit, # min flame
			acceleration_limit); # max flame
	
	state.angular_flame = angular_flame_vector; #* state.Mass;

