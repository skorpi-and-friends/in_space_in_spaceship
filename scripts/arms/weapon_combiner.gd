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
		children.append(weapon);
	damage /= len(children);


func _activate():
	# TODO: cache this stuff
	for weapon in children:
		weapon._activate();
		damage += weapon.damage;
		active = active || weapon.active;
	damage /= len(children);
