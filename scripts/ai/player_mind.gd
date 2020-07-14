extends CraftMind

class_name PlayerMind

enum InterfaceMode {
	COCKPIT,
	ORBIT
}

export var _camera_free_look := false;

onready var craft_master: CraftMaster;
onready var orbit_camera := $OrbitCamera as CraftCamera;
onready var cockpit: CockpitMaster;
var _current_craft_has_cockpit := false;

onready var _rotation_pid := PIDControllerVector.new();

# gets switched to COCKPIT in ready
export(InterfaceMode) var i_mode := InterfaceMode.ORBIT; 

func _enter_tree() -> void:
	Globals.player_mind = self;

func _ready():
	for child in get_children():
		var craft := child as CraftMaster;
		if craft:
			craft_master = craft;
			orbit_camera.target_path = craft_master.get_path();
			break;
	#setup cockpit later as CockpitMaster might not be ready at this point
	call_deferred("_setup_cockpit");


func _setup_cockpit():
	cockpit = Globals.cockpit_master;
	# cockpit is optional
	if cockpit:
		# we can only use cockpits if the craft is setup for it
		_current_craft_has_cockpit = cockpit.set_craft(craft_master);
		
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
	
	if i_mode == InterfaceMode.COCKPIT:
		if event.is_action_pressed("Toggle Immersive Cockpit"):
			cockpit.toggle_immersive_cockpit();
		
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
	
	"""
	state.linear_input = linear_input;
	var rotation_command := Vector3();
	if Input.is_action_pressed("Roll Right"):
		rotation_command.z += 90
	if Input.is_action_pressed("Roll Left"):
		rotation_command.z -= 90
	if Input.is_action_pressed("Pitch Up"):
		rotation_command.x -= 90
	if Input.is_action_pressed("Pitch Down"):
		rotation_command.x += 90
	if Input.is_action_pressed("Yaw Left"):
		rotation_command.y += 90
	if Input.is_action_pressed("Yaw Right"):
		rotation_command.y -= 90
	
	var current_rotaion = craft_master.rotation * Utility.RAD2DEG;
	rotation_command += current_rotaion;
	var angular_input := (_rotation_pid.update(
			Vector3( # state
				Utility.smallest_postive_equivalent_angle_deg(current_rotaion.x), 
				Utility.smallest_postive_equivalent_angle_deg(current_rotaion.y), 
				Utility.smallest_postive_equivalent_angle_deg(current_rotaion.z)
				),
			Vector3( # error
				Utility.delta_angle_deg(current_rotaion.x, rotation_command.x), 
				Utility.delta_angle_deg(current_rotaion.y, rotation_command.y), 
				Utility.delta_angle_deg(current_rotaion.z, rotation_command.z)
				),
			delta));
	state.angular_input = angular_input * Utility.DEG2RAD;
	
	if i_mode == InterfaceMode.ORBIT:
		
		if !_camera_free_look: # we're using the mouse for movement
			angular_input += face_dir_angular_input(
					orbit_camera.facing_direction, craft_master.global_transform); 
			graph_value = angular_input.y;
		
	
	
	"""
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
		if !_camera_free_look: # we're using the mouse for movement
			angular_input += face_dir_angular_input(
					orbit_camera.facing_direction, craft_master.global_transform); 
			graph_value = angular_input.y;
#	angular_input *= state.angular_v_limit;
#	angular_input *= delta;
	state.angular_input = angular_input;


var graph_value: float


func switch_interface_mode():
	# default to orbit if no cockpit found
	if _current_craft_has_cockpit && i_mode == InterfaceMode.ORBIT:
		cockpit.enable_cockpit();
		i_mode = InterfaceMode.COCKPIT;
	elif i_mode == InterfaceMode.COCKPIT:
		cockpit.disable_cockpit();
		orbit_camera.make_current();
		#align_orbit_camera_to_craft
		orbit_camera.facing_direction = craft_master.global_transform.basis.z;
		i_mode = InterfaceMode.ORBIT;

