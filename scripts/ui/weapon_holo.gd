extends Control

class_name WeaponHolo

var weapon: Node#: Weapon;
var name_label: String;

onready var _activation_display := find_node("Activatation", true) as ColorRect;
onready var _name_label := find_node("NameLabel", true) as Label;

func _ready() -> void:
	_name_label.text = name_label if name_label else weapon.name;

func _process(delta: float) -> void:
#	if !weapon:
#		_activation_display.color = Color.webgray;
	if weapon.active:
		_activation_display.color = Color.coral;
	else:
		_activation_display.color = Color.firebrick;