extends MarginContainer

class_name TimeGraph

onready var line := $Line as Line2D;

export var max_point_count := 100;

export var _max_value := 100;

export var values := PoolRealArray();

func _ready() -> void:
	var x_gap := rect_size.x / max_point_count;
	var points := PoolVector2Array();
	for x in max_point_count:
		values.append(0);
		points.append(Vector2(x * x_gap, rect_size.y))
	line.points = points


func push_point(value: float):
	values.remove(0);
	values.append(value);
	redraw_line();
#	if value > _max_value:
#		redraw_line();
#	else:
#		if line.get_point_count() > max_point_count:
#			line.remove_point(0);
#		var line_height = ((_max_value - value) / _max_value) * rect_size.y;
#		var line_width = (len(values) / max_point_count) * rect_size.x;
#		line.add_point(Vector2(line_width, line_height))


func redraw_line():
	var x_gap := rect_size.x / len(values)
	var index := 0;
	for value in values:
		var line_height = ((_max_value - value) / _max_value) * rect_size.y;
		line.set_point_position(index, Vector2(index * x_gap, line_height));
		index += 1;
		