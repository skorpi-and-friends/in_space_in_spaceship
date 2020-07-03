extends Node

#abstract
class_name CraftDriver

export var _linear_input_multiplier := 1.0;
export var _angular_input_multiplier := 1.0;


func _init_from_config(config: CraftConfig):
	_linear_input_multiplier = config.linear_input_multiplier;
	_angular_input_multiplier = config.angular_input_multiplier;


func _update_flames(state: CraftState):
	pass