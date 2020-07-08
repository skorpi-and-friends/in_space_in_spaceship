extends CraftMotor

# FIXME: everything
func _apply_flames(state:CraftState, rigidbody: RigidBody):
	rigidbody.add_central_force(rigidbody.to_global(state.linear_flame));
	rigidbody.add_torque(rigidbody.to_global(state.angular_flame));