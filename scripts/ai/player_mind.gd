extends CraftMind

class_name PlayerMind

var modules := [];

onready var craft_master: CraftMaster;

func _enter_tree() -> void:
	Globals.player_mind = self;
	for child in get_children():
		var module := child as PlayerMindModule;
		if module:
			modules.append(module);
			module.player_mind = self;

func _process(delta: float) -> void:
	var state = craft_master.engine.state;
	state.linear_input = Vector3();
	state.angular_input = Vector3();
	for module in modules:
		module._update_engine_input(state);


func _ready():
	for child in get_children():
		var craft := child as CraftMaster;
		if craft:
			call_deferred("take_control_of_craft", craft);
			break;
	# assert on the next frame
	yield(get_tree(), "idle_frame")
	assert(craft_master);


func _input(event: InputEvent) -> void:
	if event.is_action_pressed("Fire Primary"):
		craft_master.arms.activate_primary();
	elif event.is_action_pressed("Fire Secondary"):
		craft_master.arms.activate_secondary();
	elif event.is_action_pressed("Toggle Mouse Capture"):
		var new_mouse_mode := Input.MOUSE_MODE_CAPTURED;
		if Input.get_mouse_mode() == new_mouse_mode:
			new_mouse_mode = Input.MOUSE_MODE_VISIBLE 
		Input.set_mouse_mode(new_mouse_mode);


var graph_value: float

func take_control_of_craft(craft: CraftMaster) -> void:
	if craft == craft_master:
		return;
	craft_master = craft;
	
	for module in modules:
		module._craft_changed(craft);