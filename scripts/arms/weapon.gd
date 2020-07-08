extends Spatial

class_name Weapon

signal damage_done(weapon, node, damage);

export var damage: float;
export(Damage.Type) var damage_type := Damage.Type.DEFAULT;
export var active := false;

func _activate():
	pass


func get_weapon() -> Weapon:
	return self;


func _report_damage(node: Node, damage_done: float):
	emit_signal("damage_done", self, node, damage_done);