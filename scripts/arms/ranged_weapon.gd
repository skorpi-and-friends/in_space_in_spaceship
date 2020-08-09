extends Weapon

"""abstract"""
class_name RangedWeapon 

func _get_projectile_velocity() -> float:
	return INF;


func _get_range() -> float:
	return INF;


func _get_emit_frequency() -> float:
	return INF;
