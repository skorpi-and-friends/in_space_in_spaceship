extends Spatial

class_name Weapon

signal damage_done(weapon, node, damage);

export var damage: float;
export(Damage.Type) var damage_type := Damage.Type.DEFAULT;
export var active := false;

var _default_holo_display := preload("res://scenes/ui/weapon_holos/default.tscn");

func _activate():
	pass

func _get_holo_display() -> Control: #WeaponHolo
	var holo := _default_holo_display.instance() as Control;
	holo.weapon = self;
	return holo;


func get_weapon() -> Weapon:
	return self;


func _report_damage(node: Node, damage_done: float):
	emit_signal("damage_done", self, node, damage_done);