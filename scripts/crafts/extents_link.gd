extends Node

export var _mesh_path: NodePath;
export var _engine_path:= @"../Engine";

func _enter_tree() -> void:
	var mesh = get_node(_mesh_path) as VisualInstance;
	var engine = get_node(_engine_path) as CraftEngine;
	assert(mesh && engine);
	var size = mesh.get_aabb().size;
	if size == Vector3():
		printerr("linked mesh has zero volume.");
	engine.craft_extents = size;
	queue_free()
	
