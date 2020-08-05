extends Control

class_name CraftInterface

onready var _lvelocity_label := find_node("LinearVelocity") as Label;
onready var _avelocity_label := find_node("AngularVelocity") as Label;
onready var _setspeed_label := find_node("SetSpeed") as Label;
onready var _linput_label := find_node("LinearInput") as Label;
onready var _ainput_label := find_node("AngularInput") as Label;
onready var _lflame_label := find_node("LinearFlame") as Label;
onready var _aflame_label := find_node("AngularFlame") as Label;

onready var _any_display_label := find_node("Any") as Label;
onready var time_graph := find_node("Graph") as TimeGraph;

func _process(_delta: float) -> void:
	if !Globals.player_mind:
		return;
	var player := Globals.player_mind;
	var state := player.active_craft.engine.state;
	_lvelocity_label.text = "LVel: %s m/s" % Utility.format_vector_std(state.linear_velocity);
	_avelocity_label.text = "AVel: %s m/s" % Utility.format_vector_std(state.angular_velocity);
	_setspeed_label.text = "SSpd: %s m/s" % Utility.format_vector_std(state.set_speed);
	_linput_label.text = "LInp: %s m/s" % Utility.format_vector_std(state.linear_input);
	_ainput_label.text = "AInp: %s m/s" % Utility.format_vector_std(state.angular_input);
	_lflame_label.text = "LFlm: %s m/ss" % Utility.format_vector_std(state.linear_flame);
	_aflame_label.text = "AFlm: %s m/ss" % Utility.format_vector_std(state.angular_flame);
	return;
	var display = player.orbit_camera.facing_direction;
	_any_display_label.text = "Display: %s" %player.graph_value# display;
	time_graph.push_point(player.graph_value);
