extends Spatial

class_name CockpitMaster

onready var _displays: Array = [
	find_node("PrimaryDisplay", true),
	find_node("EngineCkpitDisplay", true),
	find_node("ArmsCkpitDisplay", true),
	find_node("AttireCkpitDisplay", true),
];

onready var _cockpit_core_nodes := $Core as Spatial;
onready var _cockpit_world_nodes := $World as Spatial;

export var _camera_path:= @"Core/CameraPivot";
onready var cockpit_camera := get_node(_camera_path) as Camera;

onready var _craft_front_camera := get_node(_camera_path) as Camera;

export var _hud_path:= @"Core/HUD";
onready var _hud := get_node(_hud_path) as Control;

export var enabled := true;
var _immersive_cockpit_marker: Position3D;
var immersive_mode_enabled := false;


func _enter_tree() -> void:
	Globals.cockpit_master = self;


func _ready() -> void:
	assert(cockpit_camera);
	assert(len(_displays) > 0);
	assert(_cockpit_core_nodes);
	assert(_cockpit_world_nodes);


# we can only use cockpits if the craft is setup for it
func set_craft(craft: CraftMaster) -> bool:
	_craft_front_camera = craft.find_node("FrontCamera") as Camera;
	if !_craft_front_camera:
		return false;
	_immersive_cockpit_marker = craft.find_node("ImmersiveCockpitMarker") as Position3D;
	for display in _displays:
		var ckpit_display := display as CockpitDisplay;
		assert(ckpit_display);
		ckpit_display.craft = craft;
		ckpit_display._ready_display();
	return true;


func enable_cockpit():
	if enabled:
		return
	visible = true;
	# we'll have to make Control nodes visible separately
	_hud.visible = true;
	_craft_front_camera.make_current();
	Globals.viewport_master.switch_to_cockpit_screen();
#	propagate_call("set_process()", [true], false);
	enabled = true;


func disable_cockpit():
	if !enabled:
		return
	if immersive_mode_enabled:
		toggle_immersive_cockpit();
	visible = false;
	_hud.visible = false;
	# FIXME: replace with a remove_child solution 
#	propagate_call("set_process()", [false], false);
	Globals.viewport_master.switch_to_game_screen();
	enabled = false;
#	set_process_internal(!is_processing_internal());"


func immersive_cockpit_availaible() -> bool:
	return _immersive_cockpit_marker != null;


func toggle_immersive_cockpit():
	if !enabled || !immersive_cockpit_availaible():
		return;
	if !immersive_mode_enabled:
		remove_child(_cockpit_core_nodes);
		_immersive_cockpit_marker.add_child(_cockpit_core_nodes);
		Globals.viewport_master.switch_to_game_screen();
		cockpit_camera.make_current();
		immersive_mode_enabled = true;
	else:
		_craft_front_camera.make_current();
		_immersive_cockpit_marker.remove_child(_cockpit_core_nodes);
		add_child(_cockpit_core_nodes);
		Globals.viewport_master.switch_to_cockpit_screen();
		immersive_mode_enabled = false;


	
	