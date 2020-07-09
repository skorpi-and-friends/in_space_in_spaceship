extends Container

class_name VBoxRespectful

"""
Looks like the normal box containers respect min size,
a property I totally didn't notice. Will remove this file
after committing, just in case
"""

func _ready() -> void:
	connect("sort_children",self,"do_magic");
enum ChildSize {AUTO, CUSTOM}

func do_magic():
	var child_count := get_child_count();
	var child_types:= {}
	for child in get_children():
		var control := child as Control;
		if !control:
			continue;
		if is_default_size(control):
			child_types[control] = ChildSize.AUTO;
		else:
			child_types[control] = ChildSize.CUSTOM;
	var custom_size_height_sum := 0.0;
	for child in child_types:
		if child_types[child] == ChildSize.CUSTOM:
			var control := child as Control;
			custom_size_height_sum += control.rect_size.x;

	var remaining_auto_height := rect_size.x - custom_size_height_sum;
	var last_y_location := 0.0;
	for child in child_types:
		var control := child as Control;
		if child_types[child] == ChildSize.CUSTOM:
			control.rect_position = Vector2(0, last_y_location);
			last_y_location += control.rect_size.x;
	# skkrrrttt


func is_default_size(control: Control) -> bool:
	return control.anchor_bottom == 0 && control.anchor_left == 0 && control.anchor_right == 0 && control.ancor_top == 0;