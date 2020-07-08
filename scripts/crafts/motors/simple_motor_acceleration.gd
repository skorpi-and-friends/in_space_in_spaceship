extends CraftMotor

class_name SimpleMotorAcceleration

func _apply_flames(state:CraftState, rigidbody: RigidBody):
	# rigidbody.add_central_force(state.linear_flame * state.mass);
	#rigidbody.add_torque(state.angular_flame * state.mass);

	# flame is holding acceleration value
	var linear_force := state.linear_flame * state.mass * Globals.MASS_MODIFIER;
	# the force vector is in local space, we need to convert it to global
	linear_force = Utility.transform_vector(rigidbody.global_transform, linear_force);
	rigidbody.add_central_force(linear_force);

	var torque := state.angular_flame * state.moment_of_inertia * Globals.MASS_MODIFIER;
	torque = Utility.transform_vector(rigidbody.global_transform, torque);
	rigidbody.add_torque(torque);
