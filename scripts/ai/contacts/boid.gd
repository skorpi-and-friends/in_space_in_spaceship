extends ScanPresence

class_name Boid

export var _body: NodePath;

func _enter_tree():
	._enter_tree();
	add_to_group("Boid")

func get_body() -> Spatial:
	return get_node(_body) as Spatial;