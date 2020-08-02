extends PlayerMindModule

class_name CameraInterface

enum InterfaceMode {
	COCKPIT,
	COCKPIT_IR,
	ORBIT
}

# gets switched to COCKPIT in ready
export(InterfaceMode) var i_mode := InterfaceMode.ORBIT;

export var _camera_free_look := false;
onready var orbit_camera := $"../OrbitCamera" as CraftCamera;

onready var cockpit: CockpitMaster;
var _current_craft_has_cockpit := false;


func _ready():
	#setup cockpit later as CockpitMaster might not be ready at this point
	switch_free_look(true);
	call_deferred("_setup_cockpit");


func _process(_delta: float) -> void:
	var state := craft_master.engine.state;
	
	if i_mode == InterfaceMode.ORBIT:
		if !_camera_free_look: # we're using the mouse for movement
			state.angular_input += PlayerMind.face_dir_angular_input(
					orbit_camera.facing_direction, craft_master.global_transform); 
			player_mind.graph_value = state.angular_input.y;
#	angular_input *= state.angular_v_limit;
#	angular_input *= delta;



func _input(event: InputEvent) -> void:
	if event.is_action_pressed("Switch Interface Mode"):
		switch_interface_mode();
	elif i_mode == InterfaceMode.ORBIT:
		if Input.is_action_just_pressed("Camera Free Look"):
			switch_free_look(true);
		elif Input.is_action_just_released("Camera Free Look"):
			switch_free_look(false);
		elif event.is_action_pressed("Toggle Camera Free Look"):
			switch_free_look();
		elif event.is_action_pressed("Increase Camera Distance"):
			orbit_camera.distance += 1;
		elif event.is_action_pressed("Decrease Camera Distance"):
			orbit_camera.distance -= 1;


func _setup_cockpit():
	cockpit = Globals.cockpit_master;


func switch_interface_mode():
		
	# default to orbit if no cockpit found
	if _current_craft_has_cockpit && i_mode == InterfaceMode.ORBIT:
		cockpit.enable_cockpit();
		i_mode = InterfaceMode.COCKPIT;
	elif _current_craft_has_cockpit && i_mode == InterfaceMode.COCKPIT && cockpit.immersive_cockpit_availaible():
		cockpit.toggle_immersive_cockpit();
		i_mode = InterfaceMode.COCKPIT_IR;
	else: # reset the orbit camera otherwise 
		if cockpit:
			cockpit.disable_cockpit();
		orbit_camera.target = craft_master;
		orbit_camera.make_current();
		# align_orbit_camera_to_craft
		orbit_camera.facing_direction = craft_master.global_transform.basis.z;
		i_mode = InterfaceMode.ORBIT;


# toggles if no args passed
func switch_free_look(on := !_camera_free_look):
	if on:
		_camera_free_look = true;
		orbit_camera.auto_align = true;
	else:
		_camera_free_look = false;
		orbit_camera.auto_align = false;


func _craft_changed(craft: CraftMaster) -> void:
	._craft_changed(craft);
	
	# cockpit is optional
	if cockpit:
		# we can only use cockpits if the craft is setup for it
		_current_craft_has_cockpit = cockpit.set_craft(craft);
	i_mode = InterfaceMode.ORBIT
	switch_interface_mode();
