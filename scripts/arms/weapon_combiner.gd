extends RangedWeapon

class_name WeaponCombiner

var children := [];

func _ready():
	damage = 0;
	for node in get_children():
		var weapon := node as RangedWeapon;
		if !weapon: continue;
		damage += weapon.damage;
		active = active || weapon.active;
		assert(weapon.connect("damage_done", self, "report_child_damage") == OK);
		children.append(weapon);
	assert(len(children) > 0);
	damage /= len(children);


func report_child_damage(child_weapon: Weapon, node: Node, damage_done: float):
	emit_signal("damage_done", child_weapon, node, damage_done);


func _process(_delta: float) -> void:
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


func _get_projectile_velocity() -> float:
	var ctr := 0.0;
	var sum := 0.0;
	for weapon in children:
		sum += weapon._get_projectile_velocity();
		ctr += 1;
	if ctr > 0:
		return sum / ctr as float; 
	return 0.0;


func _get_range() -> float:
	var ctr := 0.0;
	var sum := 0.0;
	for weapon in children:
		sum += weapon._get_range();
		ctr += 1;
	if ctr > 0:
		return sum / ctr as float; 
	return 0.0;


func _get_emit_frequency() -> float:
	var ctr := 0.0;
	var sum := 0.0;
	for weapon in children:
		sum += weapon._get_emit_frequency();
		ctr += 1;
	if ctr > 0:
		return sum / ctr as float; 
	return 0.0;
