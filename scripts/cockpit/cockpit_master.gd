extends Spatial

class_name CockpitMaster

onready var _displays:Array = [
	$"MainDisplay/Viewport/MainCkpitDsp",
	$"ArmsDisplay/Viewport/ArmsCkpitDsp",
];

export var _camera_path: NodePath;
onready var camera := get_node(_camera_path) as Camera;

export var enabled := true;

func _ready() -> void:
	assert(camera);


func set_craft(craft: CraftMaster):
	for display in _displays:
		var ckpit_display := display as CockpitDisplay;
		assert(ckpit_display);
		ckpit_display.craft = craft;
		ckpit_display._ready_display();

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
	propagate_call("set_process()", [false], false);
	enabled = false;
#		set_process_internal(!is_processing_internal());"
	