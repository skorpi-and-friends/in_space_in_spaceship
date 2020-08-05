extends PlayerMindModule

class_name EngineController

func _update_engine_input(state: CraftState) -> void:
	
	var linear_input := Vector3();
	if Input.is_action_pressed("Thrust"):
		linear_input.z += 1
	if Input.is_action_pressed("Thrust Reverse"):
		linear_input.z -= 1
	if Input.is_action_pressed("Starfe Left"):
		linear_input.x += 1
	if Input.is_action_pressed("Starfe Right"):
		linear_input.x -= 1
	if Input.is_action_pressed("Altitude Raise"):
		linear_input.y += 1
	if Input.is_action_pressed("Altitude Lower"):
		linear_input.y -= 1
	
	linear_input *= state.linear_v_limit;
	
	# use set speed if input is idle (i.e. 0) in given axis
	if !linear_input.x:
		linear_input.x = state.set_speed.x;
	# and if there's an input but no velocity limit in direction
	elif !state.limit_strafe_v:
		# set the input to inf. Since input is in the range of 0-1,
		# keyobard input won't take us past the v_limit otherwise
		linear_input.x = INF;
		
	if !linear_input.y:
		linear_input.y = state.set_speed.y;
	elif !state.limit_strafe_v:
		linear_input.x = INF;
		
	if !linear_input.z:
		linear_input.z = state.set_speed.z;
	elif !state.limit_forward_v:
		linear_input.z = INF;
	
	state.linear_input += linear_input;
	
	var angular_input := Vector3();
	if Input.is_action_pressed("Roll Right"):
		angular_input.z += 1
	if Input.is_action_pressed("Roll Left"):
		angular_input.z -= 1
	if Input.is_action_pressed("Pitch Up"):
		angular_input.x -= 1
	if Input.is_action_pressed("Pitch Down"):
		angular_input.x += 1
	if Input.is_action_pressed("Yaw Left"):
		angular_input.y += 1
	if Input.is_action_pressed("Yaw Right"):
		angular_input.y -= 1
	angular_input *= state.angular_v_limit;
	var keyboard_rotation_sensetivity := ProjectSettings.get_setting("ISIS/Controls/Craft/Keyboard Rotation Sensetivity") as float;
	if keyboard_rotation_sensetivity:
		angular_input *= keyboard_rotation_sensetivity; 
	state.angular_input += angular_input * 0.1 # get_process_delta_time();
