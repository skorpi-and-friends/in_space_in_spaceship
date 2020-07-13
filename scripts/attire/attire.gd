extends Node

class_name Attire

enum Type {HULL, ARMOUR, SHIELD};

export(Type) var type:= Type.HULL;
export var remaining_integrityPPH:= 1.0;

export var remaining_integrity := 100_000.0;
export var factory_integrity := 100_000.0;
export var recovery_rate := 0.0;

# size must match damge type count
export var damage_multipliers:= [1.0,1.0,1.0,1.0,1.0,1.0]; 

func _process(delta):
	remaining_integrityPPH = remaining_integrity / factory_integrity;
	if recovery_rate > 0:
		if remaining_integrity < factory_integrity:
			remaining_integrity += factory_integrity * recovery_rate * delta;
		else:
			remaining_integrity = factory_integrity;


func damage(damage: float, type: int) -> float:
	var actual_damage := (damage_multipliers[type] as float) * damage;

	var new_integrity := remaining_integrity - actual_damage;

	if new_integrity >= 0:
		remaining_integrity = new_integrity;
		return 0.0;

	var remaining_damage := actual_damage - new_integrity;
	remaining_damage /= damage_multipliers[type];
	return remaining_damage;


func set_resistance(resistance: float, type: int):
	damage_multipliers[type] = resistance;


#class Configuration:
