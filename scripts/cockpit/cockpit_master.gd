extends Spatial

class_name CockpitMaster

onready var _displays: Array = [
	find_node("PrimaryDisplay", true),
	find_node("EngineCkpitDisplay", true),
	find_node("ArmsCkpitDisplay", true),
	find_node("AttireCkpitDisplay", true),
];

export var _camera_path: NodePath;
onready var camera := get_node(_camera_path) as Camera;

export var enabled := true;


func _enter_tree() -> void:
	Globals.cockpit_master = self;


func _ready() -> void:
	assert(camera);
	assert(len(_displays) > 0);


# we can only use cockpits if the craft is setup for it
func set_craft(craft: CraftMaster) -> bool:
	camera = craft.find_node("FrontCamera") as Camera;
	if !camera:
		return false;
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
	$HUD.visible = true;
	propagate_call("set_process()", [true], false);
	enabled = true;


func disable_cockpit():
	if !enabled:
		return
	visible = false;
	$HUD.visible = false;
	# FIXME: replace with a remove_child solution 
	propagate_call("set_process()", [false], false);
	enabled = false;
#		set_process_internal(!is_processing_internal());"
	