extends CraftMotor2D

class_name SimpleMotorAcceleration2D

func _apply_flames(state:CraftState2D, rigidbody: RigidBody2D):
	# flame is holding acceleration value
	var linear_force := state.linear_flame * state.mass * Globals.MASS_MODIFIER;
	# the force vector is in local space, we need to convert it to global
	linear_force = rigidbody.global_transform.basis_xform(linear_force);
	linear_force *= Globals.METER2PIXEL;
	# don't use the method. Look at devlog entry
#	rigidbody.add_central_force(linear_force);
	rigidbody.applied_force = linear_force;
	
	
	var torque := state.angular_flame * state.moment_of_inertia * Globals.MASS_MODIFIER;
	rigidbody.applied_torque = torque;
