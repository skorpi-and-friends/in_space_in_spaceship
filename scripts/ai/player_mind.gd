extends CraftMind

class_name PlayerMind


onready var craft_camera := $Camera as CraftCamera;


func _process(delta):
	updatecraft_master_input(delta);
	
	if Input.is_action_pressed("Fire Primary"):
		craft_master.arms.activate_primary();
	if Input.is_action_pressed("Fire Secondary"):
		craft_master.arms.activate_secondary();
	if Input.is_action_pressed("Toggle Mouse Capture"):
		var new_mouse_mode := Input.MOUSE_MODE_CAPTURED;
		if Input.get_mouse_mode() == new_mouse_mode:
			new_mouse_mode = Input.MOUSE_MODE_VISIBLE 
		Input.set_mouse_mode(new_mouse_mode);


func updatecraft_master_input(delta):
	var state := craft_master.engine.state;
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
	
	if !linear_input.x:
		linear_input.x = state.set_speed.x;
		
	if !linear_input.y:
		linear_input.y = state.set_speed.y;
		
	if !linear_input.z:
		linear_input.z = state.set_speed.z;
	
	state.linear_input = linear_input;
	
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
#
	
#	var angular_input := face_dir_angular_input(
#			craft_camera.facing_direction, craft_master.global_transform);
#	angular_input *= state.angular_v_limit;
#	angular_input *= delta;
	
	state.angular_input = angular_input;
