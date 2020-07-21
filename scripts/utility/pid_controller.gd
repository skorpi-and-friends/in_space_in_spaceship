extends Node

class_name PIDController

export var last_state: float;
export var integrat_error: float;
export var integrat_max: float;
export var integrat_min: float;
export var proportional_gain: float;
export var integrat_gain: float;
export var differential_gain: float;

func update(state: float, 
		error:float, 
		delta_time: float) -> float:
		
	# calculate the proportional term
	var drive_vector := proportional_gain * error;
	
	# calculate the integral error
	integrat_error += error * delta_time;
	
	# clamp the integrator state to mitigate windup
	integrat_error = clamp(integrat_error, integrat_min, integrat_max);

	# calculate the integral term
	drive_vector += integrat_gain * integrat_error;

	# calculate the differential term
	drive_vector += differential_gain * ((state - last_state) * delta_time);
	
	last_state = state;

	return drive_vector;