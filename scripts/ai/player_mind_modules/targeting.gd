extends PlayerMindModule

onready var _master_mind:= get_tree().get_nodes_in_group("MasterMind")[0] as Node;
onready var _hudModule := get_node("../Hud") as HudModule;

var current_target: Spatial setget _set_current_target

func _ready() -> void:
#	if !_hudModule:
#		queue_free();
#		return;
#	var master_minds = get_tree().get_nodes_in_group("MasterMind");
#	if len(master_minds) == 0:
#		return;
#	_master_mind = master_minds[0];
	assert(_hudModule);
	assert(_master_mind);
	
	assert(OK == _master_mind.connect("ContactMade", self, "_contact_made"));
	assert(OK == _master_mind.connect("ContactLost", self, "_contact_lost"));


func _input(event: InputEvent) -> void:
	if event.is_action_pressed("Switch Target"):
		var closest_boid := _master_mind.ClosestBoidTo(active_craft.presence) as Spatial;
		if closest_boid:
			self.current_target = closest_boid;


func _set_current_target(target: Spatial) -> void:
	if current_target:
		# remove the target indicator from previous target
		_hudModule.remove_indicators(current_target);
		# add a normal inidcator instead
		_hudModule.add_indicator(current_target);
	
	current_target = target;
	# don't bother anymore if incoming value is null
	if !target:
		return;
		
	if !Globals.csharp_utility_funcs.IsScanPresence(target):
		push_error("TargetingModule: set target attempt with a non ScanPresence value");
		assert(false);
	
	_hudModule.set_target_indicator(target);


func _contact_made(_contact: Spatial) -> void:
	pass


func _contact_lost(contact: Spatial) -> void:
	if contact == current_target:
		self.current_target = null;
