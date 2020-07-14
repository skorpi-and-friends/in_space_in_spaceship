extends Node

class_name ViewportMaster

onready var game_screen := $Game as ViewportContainer;
onready var cockpit_screen := $Cockpit as ViewportContainer;

var _active_screen: ViewportContainer;

func _enter_tree() -> void:
	Globals.viewport_master = self;

func _ready() -> void:
	assert(game_screen);
	assert(cockpit_screen);
#	(cockpit_screen.get_child(0) as Viewport).gui_disable_input = true;
	switch_to_cockpit_screen();

func switch_to_game_screen():
	move_child(cockpit_screen, 0);


func switch_to_cockpit_screen():
	move_child(game_screen, 0);
