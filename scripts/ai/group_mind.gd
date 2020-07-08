extends CraftMind

class_name GroupMind

var _crafts := [];

var _hostile_contacts := [];

var _master_mind: MasterMind; 

func _ready():
	_master_mind = get_tree().get_nodes_in_group("MasterMind")[0] as MasterMind;
	assert(_master_mind);
	for child in get_children():
		if child is CraftMaster:
			_crafts.append(child);
	for contact in _master_mind.master_contact_list:
		if is_hostile_contact(contact):
			_hostile_contacts.append(contact);


func _process(delta):
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

