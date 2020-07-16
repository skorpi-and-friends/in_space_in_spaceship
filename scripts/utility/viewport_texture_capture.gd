extends Viewport

var _has_captured := false;

func _process(delta: float) -> void:
	if !_has_captured && Input.is_action_pressed("Fire Primary"):
		get_texture().get_data().save_png("res://textures/capture.png")
		_has_captured = true;