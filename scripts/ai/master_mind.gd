extends Node

class_name MasterMind

export var _master_contact_list := [];

signal contact_made(contact);
signal contact_lost(contact);

func _enter_tree():
	var scene_tree = get_tree();
	scene_tree.connect("node_added",self,"node_added");
	for contact in scene_tree.get_nodes_in_group("ScanPresence"):
		_master_contact_list.append(contact);


func node_added(node: Node):
	var presence := node as ScanPresence;
	if presence:
		_master_contact_list.append(presence);
		emit_signal("contact_made", presence);


func remove_contact(contact: MasterMind):
	var index := _master_contact_list.find(contact);
	if ~index:
		_master_contact_list.remove(index);