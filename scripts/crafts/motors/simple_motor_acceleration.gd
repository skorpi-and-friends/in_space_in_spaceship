extends CraftMotor

class_name SimpleMotorAcceleration

func _apply_flames(state:CraftState, rigidbody: RigidBody):
	# rigidbody.add_central_force(state.linear_flame * state.mass);
	#rigidbody.add_torque(state.angular_flame * state.mass);
	
	# flame is holding acceleration value
	var linear_force := state.linear_flame * state.mass;
	linear_force = Utility.transform_vector(rigidbody.global_transform, linear_force);
	rigidbody.add_central_force(linear_force);
	
	var angular_force := state.angular_flame * state.mass;
	angular_force = Utility.transform_vector(rigidbody.global_transform, angular_force);
	rigidbody.add_torque(angular_force);
