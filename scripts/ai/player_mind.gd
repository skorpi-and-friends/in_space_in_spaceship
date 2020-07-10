extends CraftMind

class_name PlayerMind

enum InterfaceMode {
	COCKPIT,
	ORBIT
}

export var _camera_free_look := false;

onready var craft_master: CraftMaster;
onready var orbit_camera := $Camera as CraftCamera;

# gets switched to COCKPIT in ready
export(InterfaceMode) var i_mode := InterfaceMode.ORBIT; 

func _ready():
	for child in get_children():
		var craft := child as CraftMaster;
		if craft:
			craft_master = craft;
			orbit_camera.target_path = craft_master.get_path();
			break;
	switch_free_look(false);
	# use the function to properly set COCKPIT mode
	switch_interface_mode();

func _process(delta):
	updatecraft_master_input(delta);

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
	if event.is_action_pressed("Switch Interface Mode"):
		switch_interface_mode();
	if i_mode == InterfaceMode.ORBIT:
		if event.is_action_pressed("Toggle Camera Free Look"):
			switch_free_look();
		if event.is_action_pressed("Camera Free Look"):
			_camera_free_look = true;
		if event.is_action_pressed("Camera Free Look"):
			_camera_free_look = false;
		if event.is_action_pressed("Increase Camera Distance"):
			orbit_camera.distance += 1;
		if event.is_action_pressed("Decrease Camera Distance"):
			orbit_camera.distance -= 1;
		if Input.is_action_just_pressed("Camera Free Look"):
			switch_free_look(true);
		if Input.is_action_just_released("Camera Free Look"):
			switch_free_look(false);


# toggles if no args passed
func switch_free_look(on := !_camera_free_look):
	if on:
		_camera_free_look = true;
		orbit_camera.auto_align = true;
	else:
		_camera_free_look = false;
		orbit_camera.auto_align = false;
		

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
	angular_input *= state.angular_v_limit;
	
	if i_mode == InterfaceMode.ORBIT:
		if !_camera_free_look:
			angular_input += face_dir_angular_input(
					orbit_camera.facing_direction, craft_master.global_transform); 
#	angular_input *= state.angular_v_limit;
#	angular_input *= delta;
	
	state.angular_input = angular_input;


func switch_interface_mode():
	var cockpit := craft_master.cockpit as CockpitMaster;
	if i_mode == InterfaceMode.ORBIT:
		cockpit.camera.make_current();
		cockpit.enable_cockpit();
		i_mode = InterfaceMode.COCKPIT;
	elif i_mode == InterfaceMode.COCKPIT:
		orbit_camera.make_current();
		cockpit.disable_cockpit();
		align_orbit_camera_to_craft();
		i_mode = InterfaceMode.ORBIT;


func align_orbit_camera_to_craft():
	orbit_camera.facing_direction = craft_master.global_transform.basis.z;