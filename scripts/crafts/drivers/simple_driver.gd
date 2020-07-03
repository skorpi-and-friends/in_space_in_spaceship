extends CraftDriver

func _update_flames(state: CraftState):
	state.linear_flame = Utility.clamp_vector_components(
			state.linear_input,
			-state.linear_thruster_force,
			state.linear_thruster_force);
	state.linear_flame *= _linear_input_multiplier;
	state.angular_flame = Utility.clamp_vector_components(
			state.angular_input,
			-state.angular_thruster_torque,
			state.angular_thruster_torque);
	state.angular_flame *= _angular_input_multiplier;