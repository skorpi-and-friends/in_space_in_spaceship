extends CockpitDisplay

class_name CkpitEngine

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
#	_lvelocity_label.parse_bbcode(
#			"[code]LVel: %s m/s[/code]" % Utility.format_vector_rich(state.linear_velocity));
#	_avelocity_label.parse_bbcode(
#			"[code]AVel: %s m/s[/code]" % Utility.format_vector_rich(state.angular_velocity));
#	_setspeed_label.parse_bbcode(
#			"[code]SSpd: %s m/s[/code]" % Utility.format_vector_rich(state.set_speed));
#	_linput_label.parse_bbcode(
#			"[code]LInp: %s m/s[/code]" % Utility.format_vector_rich(state.linear_input));
#	_ainput_label.parse_bbcode(
#			"[code]AInp: %s m/s[/code]" % Utility.format_vector_rich(state.angular_input));
#	_lflame_label.parse_bbcode(
#			"[code]LFlm: %s m/s/s[/code]" % Utility.format_vector_rich(state.linear_flame));
#	_aflame_label.parse_bbcode(
#			"[code]AFlm: %s m/s/s[/code]" % Utility.format_vector_rich(state.angular_flame));

	_format_vector_for_display(state.linear_velocity, _lvelocity_label, "LVel", "m/s");
	_format_vector_for_display(state.angular_velocity, _avelocity_label, "AVel", "m/s");
	_format_vector_for_display(state.set_speed, _setspeed_label, "SSpd", "m/s");
	_format_vector_for_display(state.linear_input, _linput_label, "LInp", "m/s");
	_format_vector_for_display(state.angular_input, _ainput_label, "AInp", "m/s");
	_format_vector_for_display(state.linear_flame, _lflame_label, "LFlm", "m/s/s");
	_format_vector_for_display(state.angular_flame, _aflame_label, "AFlm", "m/s/s");


func _format_vector_for_display(vector: Vector3,label: RichTextLabel, prefix: String, postfix:String):
	label.clear();
	label.append_bbcode("[code]");
	label.add_text(prefix);
	label.add_text(": ");
	color_vector_for_RichLabel(vector, label);
	label.add_text(" ");
	label.add_text(postfix);
#	label.append_bbcode("[/code]");
	label.pop();


func color_vector_for_RichLabel(vector: Vector3, label:RichTextLabel,
		postive_color = Color.coral,
		negative_color = Color.firebrick):
	color_float_for_RichLabel(vector.x, label, postive_color, negative_color);
	label.add_text(', ')
	color_float_for_RichLabel(vector.y, label, postive_color, negative_color);
	label.add_text(', ')
	color_float_for_RichLabel(vector.z, label, postive_color, negative_color);


func color_float_for_RichLabel(number: float, label:RichTextLabel,
		postive_color = Color.coral,
		negative_color = Color.firebrick):
	label.push_color(negative_color if sign(number) <0 else postive_color);
	label.add_text("%03d." % abs(number));
	label.add_text(("%0.3f" % number).split(".")[1]);
	label.pop();
