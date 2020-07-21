extends Node

class_name PIDControllerVector2D

export var last_state: Vector2;
export var integrat_error: Vector2;
export var integrat_max: Vector2;
export var integrat_min: Vector2;
export var proportional_gain: Vector2;
export var integrat_gain: Vector2;
export var differential_gain: Vector2;

func update(state: Vector2, 
		error:Vector2, 
		delta_time: float) -> Vector2:
		
	# calculate the proportional term
	var drive_vector := proportional_gain * error;
	
	# calculate the integral error
	integrat_error += error * delta_time;
	
	# clamp the integrator state to mitigate windup
	integrat_error = Utility.clamp_vector_components_2D(
			integrat_error, integrat_min, integrat_max);

	# calculate the integral term
	drive_vector += integrat_gain * integrat_error;

	# calculate the differential term
	drive_vector += differential_gain * ((state - last_state) * delta_time);
	
	last_state = state;

	return drive_vector;