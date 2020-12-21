extends MeshInstance

export var viewport_path := @"Viewport";

func _ready() -> void:
	var viewport: Viewport =get_node_or_null(viewport_path);
	assert(viewport,"Viewport at path %s was not found" % viewport_path);
	if (material_override):
		Utility.applyViewportTexture(material_override,viewport);
	else:
		material_override = Utility.materialFromViewportTexture(viewport);
