extends Node

class_name PIDControllerVector

export var last_state: Vector3;
export var integrat_error: Vector3;
export var integrat_max: Vector3;
export var integrat_min: Vector3;
export var proportional_gain: Vector3;
export var integrat_gain: Vector3;
export var differential_gain: Vector3;

func update(state: Vector3, 
		error:Vector3, 
		delta_time: float) -> Vector3:
		
	# calculate the proportional term
	var drive_vector := proportional_gain * error;
	
	# calculate the integral error
	integrat_error += error * delta_time;
	
	# clamp the integrator state to mitigate windup
	integrat_error = Utility.clamp_vector_components(integrat_error, integrat_min, integrat_max);

	# calculate the integral term
	drive_vector += integrat_gain * integrat_error;

	# calculate the differential term
	drive_vector += differential_gain * ((state - last_state) * delta_time);
	
	last_state = state;

	return drive_vector;