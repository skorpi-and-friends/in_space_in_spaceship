extends Node

class_name ViewportMaster

onready var _game_screen := $GameContainer as ViewportContainer;
onready var _cockpit_screen := $CockpitContainer as ViewportContainer;
onready var _game_viewport := _game_screen.get_child(0) as Viewport;
onready var _cockpit_viewport := _cockpit_screen.get_child(0) as Viewport;

var _active_screen: ViewportContainer;

func _enter_tree() -> void:
	Globals.viewport_master = self;

func _ready() -> void:
	assert(_game_screen);
	assert(_cockpit_screen);
	assert(_game_viewport);
	assert(_cockpit_viewport);
	var root_viewport := $"/root" as Viewport;
	copy_shadow_atlas_settings(root_viewport, _game_viewport);
	copy_shadow_atlas_settings(root_viewport, _cockpit_viewport);
	switch_to_game_screen();


func switch_to_cockpit_screen():
	# we'll have to toggle the conversion mode if rendering
	# on a 3d surface
	_game_viewport.keep_3d_linear = true;
	_game_viewport.set_vflip(true);
	move_child(_game_screen, 0);


func switch_to_game_screen():
	_game_viewport.keep_3d_linear = false;
	_game_viewport.set_vflip(false);
	move_child(_cockpit_screen, 0);



static func copy_shadow_atlas_settings(from: Viewport, to: Viewport):
	to.shadow_atlas_quad_0 = from.shadow_atlas_quad_0;
	to.shadow_atlas_quad_1 = from.shadow_atlas_quad_1;
	to.shadow_atlas_quad_2 = from.shadow_atlas_quad_2;
	to.shadow_atlas_quad_3 = from.shadow_atlas_quad_3;
	to.shadow_atlas_size = from.shadow_atlas_size;
