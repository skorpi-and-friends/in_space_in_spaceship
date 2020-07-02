extends CraftDriver

func _update_flames(state: CraftState):
	state.linear_flame = Utilities.clamp_vector_components(
			state.linear_input,
			-state.linear_thruster_force,
			state.linear_thruster_force);
	state.linear_flame *= _linear_input_multiplier;
	state.angular_flame = Utilities.clamp_vector_components(
			state.angular_input,
			-state.angular_thruster_force,
			state.angular_thruster_force);
	state.angular_flame *= _angular_input_multiplier;