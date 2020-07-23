extends Spatial

class_name SideCraft

var members := [];
var self_craft: CraftMaster

func _enter_tree() -> void: # move sidecraft before ready call
	self_craft = get_parent() as CraftMaster;
	assert(self_craft);
	for child in self_craft.get_children():
		var side_craft := child as CraftMaster;
		if !side_craft:
			continue;
		self_craft.remove_child(side_craft);
		add_child(side_craft);
		members.append(side_craft);
		side_craft.call_deferred("was_attached", self_craft);


func is_sidecraft_member(craft: CraftMaster) -> bool:
	return members.count(craft) > 0;


func is_craft_attached(craft: CraftMaster) -> bool:
	return get_children().count(craft) > 0;


func add_sidecraft(craft: CraftMaster) -> void:
	if is_sidecraft_member(craft):
		return;
	members.append(craft);


func attach_sidecraft(craft: CraftMaster) -> bool:
	var craft_parent := craft.get_parent();
	if craft_parent == self:
		return true;
	craft_parent.remove_child(craft_parent);
	add_child(craft);
	if !is_sidecraft_member(craft):
		members.append(craft);
	craft.was_attached(self_craft);
	return true;

func detach_sidecraft(craft: CraftMaster) -> void:
	if !is_craft_attached(craft):
		return;
	remove_child(craft);
	self_craft.get_parent().add_child(craft);
	craft.was_detached(self_craft);
	