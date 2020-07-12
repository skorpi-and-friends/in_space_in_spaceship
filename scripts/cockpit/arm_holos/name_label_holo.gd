extends WeaponHolo;

class_name NameLabelHolo
	
var weapon_name: String;
	
onready var _name_label := find_node("NameLabel", true) as Label;

func _ready() -> void:
	_name_label.text = weapon_name if weapon_name else weapon.name;

