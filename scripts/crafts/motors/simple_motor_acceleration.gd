extends CraftMotor

class_name SimpleMotorAcceleration

func _apply_flames(state:CraftState, rigidbody: RigidBody):
	# rigidbody.add_central_force(state.linear_flame * state.mass);
	#rigidbody.add_torque(state.angular_flame * state.mass);
#	var delta_time = get_physics_process_delta_time()

	# DebugDraw.call("draw_ray_3d", rigidbody.global_transform.origin, state.linear_flame.normalized(), 2000, Color.aqua);

	# flame is holding acceleration value
	var linear_force := state.linear_flame * state.mass * Globals.MASS_MODIFIER;
	# the force vector is in local space, we need to convert it to global
	linear_force = Utility.transform_direction(rigidbody.global_transform, linear_force);
#	linear_force *= delta_time;

	# DebugDraw.call("draw_ray_3d", rigidbody.global_transform.origin, linear_force.normalized(), 2000, Color.yellow);
	# DebugDraw.call("draw_ray_3d", rigidbody.global_transform.origin, state.linear_velocity.normalized(), state.linear_velocity.length(), Color.yellow);
	
	rigidbody.add_central_force(linear_force);

	var torque := state.angular_flame * state.moment_of_inertia * Globals.MASS_MODIFIER;
	torque = Utility.transform_vector(rigidbody.global_transform, torque);
#	torque *= delta_time;
	rigidbody.add_torque(torque);
