extends Node

#abstract
class_name CraftDriver2D

export var _linear_input_multiplier := 1.0;
export var _angular_input_multiplier := 10.0;


func _update_flames(_state: CraftState2D) -> void:
	pass


func _moi_changed(_state: CraftState2D) -> void:
	pass
