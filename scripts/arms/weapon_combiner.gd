extends Weapon

class_name WeaponCombiner

var children := [];

func _ready():
	damage = 0;
	for node in get_children():
		var weapon := node as Weapon;
		if !weapon: continue;
		damage += weapon.damage;
		active = active || weapon.active;
		weapon.connect("damage_done", self, "report_child_damage");
		children.append(weapon);
	damage /= len(children);

func report_child_damage(child_weapon: Weapon, node: Node, damage_done: float):
	emit_signal("damage_done", child_weapon, node, damage_done);

func _process(delta: float) -> void:
	active = false;
	for weapon in children:
		active = active || weapon.active;

func _activate():
	# TODO: cache this stuff
	active = false;
	for weapon in children:
		weapon._activate();
		damage += weapon.damage;
		active = active || weapon.active;
	damage /= len(children);

