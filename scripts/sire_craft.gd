extends Spatial

class_name SireCraft

var members := [];
var _attached_transformers := [];
var mother_craft: MotherCraftMaster

func _enter_tree() -> void: 
	mother_craft = get_parent() as MotherCraftMaster;
	assert(mother_craft);
	var mother_craft_parent:= mother_craft.get_parent();
	for child in mother_craft.get_children():
		var child_craft := child as ChildCraftMaster;
		if !child_craft:
			continue;
			
		"""
		Side craft are designated in the editor by parenting 
		them to the mothercraft but we don't want parenting
		during runtime so move the sidecars to be siblings of
		their mothercraft
		"""
		# moving nodes doesn't work if they're not ready
		# it seems 
		mother_craft.call_deferred("remove_child",child_craft);
		mother_craft_parent.call_deferred("add_child", child_craft);
		
		var remote_transformer := SireCraftTransformer.new(child_craft);
		_attached_transformers.append(remote_transformer);
		add_child(remote_transformer);
		
		members.append(child_craft);
		child_craft.call_deferred("was_attached");


func is_member(craft: ChildCraftMaster) -> bool:
	return members.count(craft) > 0;


func is_craft_attached(craft: ChildCraftMaster) -> bool:
	for remote_transfomer in _attached_transformers:
		if remote_transfomer.is_transforming_craft(craft):
			return true;
	return false;


func add_member(craft: ChildCraftMaster) -> void:
	if is_member(craft):
		return;
	members.append(craft);


func attach_craft(craft: ChildCraftMaster) -> bool:
	if !is_member(craft):
		members.append(craft);
	elif is_craft_attached(craft):
		return true;
	
	var remote_transformer := SireCraftTransformer.new(craft);
	_attached_transformers.append(remote_transformer);
	add_child(remote_transformer);
	
	craft.was_attached();
	return true;


func detach_craft(craft: ChildCraftMaster) -> void:
	var transformer_idx := -1;
	
	var idx_ctr := 0;
	for remote_transfomer in _attached_transformers:
		if remote_transfomer.is_transforming_craft(craft):
			transformer_idx = idx_ctr;
			break;
		idx_ctr += 1;

	if !~transformer_idx: # craft wasn't attached
		return;
	
	_attached_transformers[transformer_idx].queue_free();
	_attached_transformers.remove(transformer_idx);
	craft.was_detached();