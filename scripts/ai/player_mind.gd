extends CraftMind

class_name PlayerMind

export var camera_free_look := false;

onready var craft_camera := $Camera as CraftCamera;


func _process(delta):
	updatecraft_master_input(delta);
	
	if Input.is_action_just_pressed("Camera Free Look"):
		camera_free_look = true;
	
	if Input.is_action_just_released("Camera Free Look"):
		camera_free_look = false;

func _input(event: InputEvent):
	if event.is_action_pressed("Fire Primary"):
		craft_master.arms.activate_primary();
	if event.is_action_pressed("Fire Secondary"):
		craft_master.arms.activate_secondary();
	if event.is_action_pressed("Toggle Mouse Capture"):
		var new_mouse_mode := Input.MOUSE_MODE_CAPTURED;
		if Input.get_mouse_mode() == new_mouse_mode:
			new_mouse_mode = Input.MOUSE_MODE_VISIBLE 
		Input.set_mouse_mode(new_mouse_mode);
	if event.is_action_pressed("Toggle Camera Free Look"):
		camera_free_look = !camera_free_look;
	if event.is_action_pressed("Camera Free Look"):
		camera_free_look = true;
	if event.is_action_pressed("Camera Free Look"):
		camera_free_look = false;
			
#	if Input.is_action_just_released("Camera Free Look"):
#		camera_free_look = false;


# we handle craft input separte from the event pipeline
# to simulate idle craft behavior
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
	
	# use set speed if input is idle (i.e. 0) in given axis
	if !linear_input.x:
		linear_input.x = state.set_speed.x;
		
	if !linear_input.y:
		linear_input.y = state.set_speed.y;
		
	if !linear_input.z:
		linear_input.z = state.set_speed.z;
	
	state.linear_input = linear_input;
	
	var angular_input := Vector3();
	if camera_free_look:
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
	else:
		angular_input = face_dir_angular_input(
				craft_camera.facing_direction, craft_master.global_transform);
#	angular_input *= state.angular_v_limit;
#	angular_input *= delta;
	
	state.angular_input = angular_input;
