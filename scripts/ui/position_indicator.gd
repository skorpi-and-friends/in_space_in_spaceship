extends Control

class_name PositionIndicator

onready var _onscreen_marker := $OnscreenMarker as Control;
onready var _offscreen_marker := $OffscreenMarker as Sprite;
#onready var _top_color_bar := $OnscreenMarker/TopColorBar as ColorRect;

onready var target: Spatial setget _set_target;

export var onscreen_marker_max_size := Vector2(64, 64);
export var onscreen_marker_min_size := Vector2(8, 8);

#var target_visiblity:= VisibilityNotifier.new();

func _ready() -> void:
	assert(_onscreen_marker);
	assert(_offscreen_marker);
#	assert(_top_color_bar);


func _process(_delta: float) -> void:
	if !target:
		return;
	var target_world_position := target.global_transform.origin;
	var viewport := get_viewport();
	var current_camera := viewport.get_camera();
	var target_position := current_camera.unproject_position(target_world_position);
	
	var is_on_screen := target_position.x > 0 && target_position.x < viewport.size.x;
	is_on_screen = is_on_screen && target_position.y > 0 && target_position.y < viewport.size.y;
	var is_behind_camera := current_camera.is_position_behind(target_world_position);
	if is_on_screen && !is_behind_camera:
		_offscreen_marker.visible = false;
		_onscreen_marker.visible = true;
		
		# todo: max indicator range
		
		var target_distance := (target_world_position - current_camera.global_transform.origin).length();
		
		var weight := (target_distance/ 2000.0); # fixme: find a sane distance
		weight = clamp(weight, 0, 1);
		# use smoothstep to prefer smaller sizes to larger sizes
		var result = lerp( 
			onscreen_marker_max_size, 
			onscreen_marker_min_size, 
			weight
		);
		
		_onscreen_marker.rect_size = result;
		
		target_position -= _onscreen_marker.rect_size * .5;
		_onscreen_marker.rect_position = target_position;
	else:
		_offscreen_marker.visible = true;
		_onscreen_marker.visible = false;
		
		var new_position := target_position;

		var half_size = viewport.size * .5;
		
		# make the origin to center of screen
		new_position -= half_size;
		
		# invert the projection is from behind
		if is_behind_camera:
			new_position *= -1;
			
		# avoid division by zero
		if new_position.x == 0:
			new_position.x += 1;
		
		var slope = new_position.y / new_position.x;
		
		# if x is longer
		if abs(slope) < 1:
			if new_position.x < 0:
				# hug the left edge
				new_position.x = -half_size.x;
			else:
				# hug the right edge
				new_position.x = half_size.x;
			# use slope to find y
			new_position.y = slope * new_position.x;
		# if y is longer or they are equal 
		else:
			if new_position.y < 0:
				# hug the top
				new_position.y = -half_size.y;
			else:
				# hug the bottom
				new_position.y = half_size.y;
			# use slope to find x
			new_position.x = new_position.y/slope;
		
		# return origin to original position
		new_position += half_size;
		
		var marker_size = _offscreen_marker.get_rect().size * _offscreen_marker.scale;
		var padding = marker_size.x;
		if marker_size.y > padding:
			padding = marker_size.y
		padding *= .5;

		# look at before clamping
		_offscreen_marker.look_at(new_position);
		
		# clamp it with padding
		new_position = Vector2(
				clamp(new_position.x, padding, viewport.size.x - padding),
				clamp(new_position.y, padding, viewport.size.y - padding)
			);
		_offscreen_marker.position = new_position;


func set_color_scheme(color: Color) -> void:
	_offscreen_marker.self_modulate = color;
	_onscreen_marker.self_modulate = color;
#	_top_color_bar.visible = true;
#	_top_color_bar.color = color;


func _set_target(value: Spatial) -> void:
	target = value;
	set_color_scheme(Color.white);
#	_top_color_bar.visible = false;
