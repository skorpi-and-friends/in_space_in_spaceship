extends Node

class_name PlayerMindModule

const identifier_meta = "player_mind_module";

var active_craft: CraftMaster;
var player_mind setget _set_player_mind;

func _init() -> void:
	set_meta(identifier_meta, true);


func _craft_changed(craft: CraftMaster) -> void:
	active_craft = craft;


func _set_player_mind(mind):# PlayerMind):
	player_mind = mind;


func _update_engine_input(_state: CraftState) -> void:
	pass
