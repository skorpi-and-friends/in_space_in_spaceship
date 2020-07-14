#tool
extends CockpitDisplay

class_name CkpitPrimary

onready var _screen := $Screen as MeshInstance;

func _ready_display() -> void:
#	material_override.albedo_texture = game_viewport.get_texture();
	_screen.material_override.emission_texture = craft.get_viewport().get_texture();