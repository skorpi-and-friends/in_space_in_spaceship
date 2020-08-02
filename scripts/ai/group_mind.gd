extends CraftMind

class_name GroupMind

var _master_mind: MasterMind;

var _crafts := [];
var _hostile_contacts := []; 

var _active_craft_index := 0;
var _craft_targets := {};

onready var _all_follow_tree := $AllFollow as BehaviorTree;

func _ready():
	_master_mind = get_tree().get_nodes_in_group("MasterMind")[0] as MasterMind;
	assert(_master_mind);
	for child in get_children():
		if child is CraftMaster:
			_crafts.append(child);
	for contact in _master_mind.master_contact_list:
		if is_hostile_contact(contact):
			_hostile_contacts.append(contact);


func _process(_delta: float) -> void:
	_all_follow_tree.root_node.full_tick();
	return;
	if _hostile_contacts.empty():
		return;
	var enemy := (_hostile_contacts[0] as Boid).get_body();
	for craft_node in _crafts:
		var craft := craft_node as CraftMaster;
		var hostile_direction := enemy.translation - craft.translation;
		craft.engine.state.angular_input = face_dir_angular_input(
				hostile_direction, craft.global_transform);


func new_contact(contact: ScanPresence):
	if is_hostile_contact(contact):
		_hostile_contacts.append(contact);


func is_hostile_contact(contact: ScanPresence) -> bool:
#	return contact.is_in_group("Boid") && self.is_a_parent_of(contact);
	return contact is Boid && !self.is_a_parent_of(contact);


var _target_situation_changed := true;

func _has_target_situation_changed() -> bool:
	if _target_situation_changed:
		_target_situation_changed= false;
		return true;
	return false;


func _distribute_targets() -> int:
	var enemy = _hostile_contacts[0];
	for craft in _crafts:
		_craft_targets[craft.name] = enemy;
	return GreenBehaviors.NodeState.SUCCESS;


func _activate_next_craft() -> int: # notice! this has an inverter
	var craft_count := len(_crafts);
	if craft_count == 0:
		return GreenBehaviors.NodeState.SUCCESS;
	_active_craft_index = wrapi(
		_active_craft_index + 1, 0, craft_count);
	return GreenBehaviors.NodeState.FAILURE;


func _craft_has_target() -> bool:
	var craft_name = _crafts[_active_craft_index].name;
	if _craft_targets.has(craft_name):
		if _craft_targets[craft_name] != null:
			return true;
	return false


func _follow_target() -> int:
	var craft := _crafts[_active_craft_index] as CraftMaster;
	var enemy := (_craft_targets[craft.name] as Boid).get_body();
	var hostile_direction := enemy.translation - craft.translation;
	craft.engine.state.angular_input = face_dir_angular_input(
			hostile_direction, craft.global_transform);
	craft.engine.state.linear_input = hostile_direction.normalized() * craft.engine.state.linear_v_limit;
	return GreenBehaviors.NodeState.SUCCESS;


func _craft_lost_target() -> int:
	_target_situation_changed = true;
	return GreenBehaviors.NodeState.SUCCESS;
