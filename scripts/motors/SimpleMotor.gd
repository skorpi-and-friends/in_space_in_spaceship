extends CraftMotor

func _apply_flames(state:CraftState, rigidbody: RigidBody):
	rigidbody.add_central_force(state.linear_flame);
	rigidbody.add_torque(state.angular_flame);