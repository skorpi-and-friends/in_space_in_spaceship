extends Spatial

class_name ArmamentMaster

onready var primary_weapon := get_node_or_null(@"Primary") as Weapon
onready var secondary_weapon := get_node_or_null(@"Secondary") as Weapon

func activate_primary():
	if primary_weapon:
		primary_weapon._activate();


func activate_secondary():
	if secondary_weapon:
		secondary_weapon._activate();


func get_all_weapons() -> Array:
	var weapons := [];
	if primary_weapon:
		weapons.append(primary_weapon);
		
	if secondary_weapon:
		weapons.append(secondary_weapon);
	return weapons;
