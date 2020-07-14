extends Node

class_name ViewportMaster

onready var game_screen := $Game as ViewportContainer;
onready var cockpit_screen := $Cockpit as ViewportContainer;
onready var _game_viewport := game_screen.get_child(0) as Viewport;
onready var _cockpit_viewport := cockpit_screen.get_child(0) as Viewport;

var _active_screen: ViewportContainer;

func _enter_tree() -> void:
	Globals.viewport_master = self;

func _ready() -> void:
	assert(game_screen);
	assert(cockpit_screen);
	assert(_game_viewport);
	assert(_cockpit_viewport);
#	(cockpit_screen.get_child(0) as Viewport).gui_disable_input = true;
	var root_viewport := $"/root" as Viewport;
	copy_shadow_atlas_settings(root_viewport, _game_viewport);
	copy_shadow_atlas_settings(root_viewport, _cockpit_viewport);
	switch_to_cockpit_screen();


func switch_to_cockpit_screen():
	# we'll have to toggle the conversion mode if rendering
	# on a 3d surface
	var game_viewport :=(game_screen.get_child(0) as Viewport);
	game_viewport.keep_3d_linear = true;
	game_viewport.set_vflip(true);
	move_child(game_screen, 0);


func switch_to_game_screen():
	var game_viewport :=(game_screen.get_child(0) as Viewport);
	game_viewport.keep_3d_linear = false;
	game_viewport.set_vflip(false);
	move_child(cockpit_screen, 0);


func copy_shadow_atlas_settings(from: Viewport, to: Viewport):
	to.shadow_atlas_quad_0 = from.shadow_atlas_quad_0;
	to.shadow_atlas_quad_1 = from.shadow_atlas_quad_1;
	to.shadow_atlas_quad_2 = from.shadow_atlas_quad_2;
	to.shadow_atlas_quad_3 = from.shadow_atlas_quad_3;
	to.shadow_atlas_size = from.shadow_atlas_size;