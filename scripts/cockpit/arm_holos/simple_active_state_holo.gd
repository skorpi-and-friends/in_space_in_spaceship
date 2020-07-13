extends NameLabelHolo

onready var _activation_display := find_node("Activatation", true) as ColorRect;


func _process(delta: float) -> void:
#	if !weapon:
#		_activation_display.color = Color.webgray;
	if weapon.active:
		_activation_display.color = Color.coral;
	else:
		_activation_display.color = Color.firebrick;
