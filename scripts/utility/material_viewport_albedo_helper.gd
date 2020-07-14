extends MeshInstance

export var viewport_path := @"Viewport";

func _ready() -> void:
	material_override.albedo_texture = (get_node(viewport_path) as Viewport).get_texture();