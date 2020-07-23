extends Node

class_name PlayerMindModule

var craft_master: CraftMaster;
var player_mind# PlayerMind
	
func _craft_changed(craft: CraftMaster) -> void:
	craft_master = craft;


func _update_engine_input(state: CraftState) -> void:
	pass