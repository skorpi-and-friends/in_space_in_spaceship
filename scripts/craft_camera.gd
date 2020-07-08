extends Camera

class_name CraftCamera

export var default_facing := Vector3(0, 0.66, -1);

export var target_path: NodePath;

export var distance := 30.0;

export var facing_direction := Vector3(0.0, 1.0, -1.0);
export var rotation_speed := 5.0 / 1000.0;
export var auto_align := true;

export(float, 0, 90, 0.1) var align_delay := 1.5;
export(float, 0, 90, 0.1) var align_smooth_range := 45.0;

var _last_manual_rotation_time: float;
var _focus_point: Vector3;
var _previous_focus_point: Vector3;

#var _rotation_changed_manually := false;
var _target: Spatial;

var _target_rotation: Basis;
#var _x_manual_motion_flipped := false; 

func _process(delta: float):
	update_state(delta);
	facing_direction = facing_direction.normalized();
	var target_up := _target_rotation.y.normalized();

#	facing_direction = facing_direction.rotated(Vector3.UP, 
#			Utility.delta_angle(_target.rotation.y, rotation.y));
	
	# don't mess with manual alignment if not in motion
	var has_moved := (_focus_point - _previous_focus_point).length_squared() > 0.0001;
	if _last_manual_rotation_time > align_delay && has_moved:
		auto_align = false;
	
	if auto_align:
		# adjust rotation to align with target's roll
		var adjusted_rotation := Utility.transform_direction(
				_target_rotation, default_facing);
				
		# do automatic rotation
		facing_direction = lerp(
				facing_direction, adjusted_rotation.normalized(), rotation_speed * 10).normalized();

	
	# face the other way since camera forward is reversed
	var new_rotation := Utility.look_direction_basis(
				-facing_direction, target_up);
	var new_position := _focus_point + (facing_direction * distance);
	
#	look_at_from_position(new_position, _focus_point, target_up);
	global_transform = Transform(new_rotation, new_position);


func update_state(delta: float):
	# TODO: target caching
	_target = get_node(target_path) as Spatial;
	
	_last_manual_rotation_time += delta;
	
	_previous_focus_point = _focus_point;
	_focus_point = _target.global_transform.origin;
	
	_target_rotation = _target.global_transform.basis;


func _input(event):
	var mouse_event := event as InputEventMouseMotion;
	if !mouse_event:
		return;
	
	var mouse_motion := -mouse_event.relative;
	
	mouse_motion *= (rotation_speed * get_process_delta_time());
	
	# FIXME: why does this better when using the
	# cameras axes instead of the target's axes?
	var new_dir := facing_direction.rotated(
			# notice the axis switch
			global_transform.basis.y, 
			mouse_motion.x);
	new_dir = new_dir.rotated(
			global_transform.basis.x, mouse_motion.y);
	
	# clamp manual motion to the poles
	# check if abs(direction_transformed_by_target.y) == 1
	if 1 - abs(_target_rotation.xform(new_dir).y) < 0.05: 
		return;
	facing_direction = new_dir;
	
	
	# disable auto align so that the manual align
	# will be respected until the timer turns it on
	 
	auto_align = false;
	_last_manual_rotation_time = 0;