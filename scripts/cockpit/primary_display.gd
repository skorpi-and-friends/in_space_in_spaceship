#tool
extends CockpitDisplay

class_name CkpitPrimary

onready var _screen := $Screen as MeshInstance;

func _ready_display() -> void:
#	_screen.material_override.emission_texture = craft.get_viewport().get_texture();
	if (_screen.material_override):
		Utility.applyViewportTexture(_screen.material_override,craft.get_viewport());
	else:
		_screen.material_override = Utility.materialFromViewportTexture(craft.get_viewport());

