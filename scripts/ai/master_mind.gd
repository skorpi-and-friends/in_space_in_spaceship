extends Node

class_name MasterMind

export var master_contact_list := [];

signal contact_made(contact);
signal contact_lost(contact);

func _ready():
	add_to_group("MasterMind");
	var scene_tree = get_tree();
	assert(scene_tree.connect("node_added",self,"node_added") == OK);
	for contact in scene_tree.get_nodes_in_group("ScanPresence"):
		master_contact_list.append(contact);


func node_added(node: Node):
	if node.is_in_group("ScanPresence"):
		master_contact_list.append(node);
		emit_signal("contact_made", node);


func remove_contact(contact: MasterMind):
	var index := master_contact_list.find(contact);
	if ~index:
		master_contact_list.remove(index);
