extends Camera

class_name CraftCamera

export var default_facing := Vector3(0, 0.66, -1);

export var target_path: NodePath;

export var distance := 30.0;

export var facing_direction := Vector3(0.0, 1.0, -1.0);

export var rotation_speed := 3.0 / 1000.0;
export(float, 0, 90, 0.1) var align_delay := 3.0;
export(float, 0, 90, 0.1) var align_smooth_range := 45.0;

var _last_manual_rotation_time: float;
var _focus_point: Vector3;
var _previous_focus_point: Vector3;

var _rotation_changed_manually := false;
var _target: Spatial;

var target_rotation: Basis;

var display;


func _process(delta: float):
	update_state(delta);
	facing_direction = facing_direction.normalized();
	display = facing_direction;
	var target_up := target_rotation.y.normalized();

#	facing_direction = facing_direction.rotated(Vector3.UP, 
#			Utility.delta_angle(_target.rotation.y, rotation.y));
	
	if _last_manual_rotation_time > align_delay:
		# do automatic rotation
		facing_direction = lerp(
				facing_direction, default_facing.normalized(), rotation_speed * 10).normalized();
	
	# adjust rotation to align with target's roll
	var adjusted_rotation := Utility.transform_direction(
			target_rotation, facing_direction);
	
	# use negative adjusted rotation since camera forward is reversed
	var new_rotation := Utility.look_direction_basis(
				-adjusted_rotation, target_up);
	var new_position := _focus_point + (adjusted_rotation * distance);
	
#	look_at_from_position(new_position, _focus_point, target_up);
	global_transform = Transform(new_rotation, new_position);
	
	_rotation_changed_manually = false;


func update_state(delta: float):
	_target = get_node(target_path) as Spatial;
	_last_manual_rotation_time += delta;
	
	_previous_focus_point = _focus_point;
	_focus_point = _target.global_transform.origin;
	
	target_rotation = _target.global_transform.basis;


func _input(event):
	var mouse_event := event as InputEventMouseMotion;
	if !mouse_event: return;
	var input := Vector2(
		mouse_event.relative.x,
		mouse_event.relative.y
	);
	input *= (rotation_speed * get_process_delta_time());
	
	facing_direction = facing_direction.rotated(
			Vector3.UP, rad2deg(input.x));
	facing_direction = facing_direction.rotated(
			Vector3.RIGHT, rad2deg(input.y));
	_rotation_changed_manually = true;
	_last_manual_rotation_time = 0;