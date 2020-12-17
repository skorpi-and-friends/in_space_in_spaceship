extends PlayerMindModule

class_name CameraInterface

enum InterfaceMode {
	COCKPIT,
	COCKPIT_IR,
	ORBIT
}

export(InterfaceMode) var i_mode := InterfaceMode.COCKPIT;

export var _camera_free_look := false;
onready var orbit_camera := $"../OrbitCamera" as CraftCamera;

onready var cockpit: CockpitMaster;
var _current_craft_has_cockpit := false;


func _ready():
	switch_free_look(true);

	#grab cockpit later as CockpitMaster might not be ready at this point
	yield(get_tree(),"idle_frame");
	cockpit = Globals.cockpit_master;
	switch_interface_mode(InterfaceMode.ORBIT);


func _process(_delta: float) -> void:
	var state := active_craft.engine.state;
	
	if i_mode == InterfaceMode.ORBIT:
		if !_camera_free_look: # we're using the mouse for movement
			state.angular_input += PlayerMind.face_dir_angular_input(
					orbit_camera.facing_direction, active_craft.global_transform); 
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


func switch_interface_mode(mode: int = (i_mode +1)% (InterfaceMode.ORBIT +1)):
	if !cockpit:
		mode = InterfaceMode.ORBIT;
	match mode:
		InterfaceMode.COCKPIT:
			cockpit.enable_cockpit();
			cockpit.toggle_immersive_cockpit(false);
		InterfaceMode.COCKPIT_IR:
			cockpit.enable_cockpit();
			cockpit.toggle_immersive_cockpit(true);
		_:
			# reset the orbit camera otherwise 
			if cockpit:
				cockpit.disable_cockpit();
			orbit_camera.target = active_craft;
			orbit_camera.make_current();
			# align_orbit_camera_to_craft
			orbit_camera.facing_direction = active_craft.global_transform.basis.z;
	i_mode = mode;
		

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
	switch_interface_mode(i_mode);
