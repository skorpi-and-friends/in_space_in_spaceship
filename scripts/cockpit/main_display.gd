extends CockpitDisplay

class_name CkpitMain

onready var _lvelocity_label := find_node("LinearVelocity") as RichTextLabel;
onready var _avelocity_label := find_node("AngularVelocity") as RichTextLabel;
onready var _setspeed_label := find_node("SetSpeed") as RichTextLabel;
onready var _linput_label := find_node("LinearInput") as RichTextLabel;
onready var _ainput_label := find_node("AngularInput") as RichTextLabel;
onready var _lflame_label := find_node("LinearFlame") as RichTextLabel;
onready var _aflame_label := find_node("AngularFlame") as RichTextLabel;

func _process(delta):
	if !craft:
		return;
	var state := craft.engine.state;
	_lvelocity_label.parse_bbcode(
			"[code]LVel: %s m/s[/code]" % Utility.format_vector_rich(state.linear_velocity));
	_avelocity_label.parse_bbcode(
			"[code]AVel: %s m/s[/code]" % Utility.format_vector_rich(state.angular_velocity));
	_setspeed_label.parse_bbcode(
			"[code]SSpd: %s m/s[/code]" % Utility.format_vector_rich(state.set_speed));
	_linput_label.parse_bbcode(
			"[code]LInp: %s m/s[/code]" % Utility.format_vector_rich(state.linear_input));
	_ainput_label.parse_bbcode(
			"[code]AInp: %s m/s[/code]" % Utility.format_vector_rich(state.angular_input));
	_lflame_label.parse_bbcode(
			"[code]LFlm: %s m/s/s[/code]" % Utility.format_vector_rich(state.linear_flame));
	_aflame_label.parse_bbcode(
			"[code]AFlm: %s m/s/s[/code]" % Utility.format_vector_rich(state.angular_flame));

