extends MarginContainer

class_name CraftInterface

export var _craft: NodePath
var _state: CraftState

onready var _lvelocity_label := find_node("LinearVelocity") as Label;
onready var _avelocity_label := find_node("AngularVelocity") as Label;
onready var _setspeed_label := find_node("SetSpeed") as Label;
onready var _linput_label := find_node("LinearInput") as Label;
onready var _ainput_label := find_node("AngularInput") as Label;
onready var _lflame_label := find_node("LinearFlame") as Label;
onready var _aflame_label := find_node("AngularFlame") as Label;

func _process(delta):
	var state:= get_craft_state();
#	_lvelocity_label.text = "LVel: %+3.2f m/s" % state.linear_velocity;
#	_avelocity_label.text = "AVel: %+3.2f m/s" % state.angular_velocity;
#	_setspeed_label.text = "SSpd: %+3.2f m/s" % state.set_speed;
#	_linput_label.text = "LInp: %+3.2f m/s" % state.linear_input;
#	_ainput_label.text = "AInp: %+3.2f m/s" % state.angular_input;
#	_lflame_label.text = "LFlm: %+3.2f m/ss" % state.linear_flame;
#	_aflame_label.text = "AFlm: %+3.2f m/ss" % state.angular_flame;

	_lvelocity_label.text = "LVel: %s m/s" % Utility.format_vector_std(state.linear_velocity);
	_avelocity_label.text = "AVel: %s m/s" % Utility.format_vector_std(state.angular_velocity);
	_setspeed_label.text = "SSpd: %s m/s" % Utility.format_vector_std(state.set_speed);
	_linput_label.text = "LInp: %s m/s" % Utility.format_vector_std(state.linear_input);
	_ainput_label.text = "AInp: %s m/s" % Utility.format_vector_std(state.angular_input);
	_lflame_label.text = "LFlm: %s m/ss" % Utility.format_vector_std(state.linear_flame);
	_aflame_label.text = "AFlm: %s m/ss" % Utility.format_vector_std(state.angular_flame);

func get_craft_state() -> CraftState:
	if !_state:
		_state = get_node(_craft).get_node("State") as CraftState;
		# TODO: remove this assert
		assert(_state != null);
	return _state;
