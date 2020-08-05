extends RemoteTransform

class_name SireCraftTransformer


func _init(craft: CraftMaster):
	call_deferred("set_path_to", craft);


func set_path_to(craft: CraftMaster):
	global_transform = craft.global_transform;
	remote_path = craft.get_path();
	name = craft.name;


func is_transforming_craft(craft: CraftMaster) -> bool:
	return name == craft.name;
