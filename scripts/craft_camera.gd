extends Camera

class_name CraftCamera

export var default_facing := Vector3(0, 0, 1);
export var facing_offset := Vector3(0, -0.166, 0);
export var position_offset := Vector3(0, 1, 0);

export var target_path: NodePath;

export var distance := 30.0;

export var facing_direction := Vector3(0.0, 0.0, 1.0);
export var rotation_speed := 5;
export var auto_align := false;

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
	if !target_path:
		return;
	update_state(delta);
	var target_up := _target_rotation.y.normalized();
	
	# don't mess with manual alignment if not in motion
	var has_moved := (_focus_point - _previous_focus_point).length_squared() > 0.0001;

	if auto_align && has_moved && _last_manual_rotation_time > align_delay:
		# adjust rotation to align with target's roll
		var adjusted_rotation := _target_rotation * default_facing;
		
		facing_direction = Quat(Utility.get_basis_facing_direction(facing_direction)
				).slerp(Quat(Utility.get_basis_facing_direction(adjusted_rotation)),
				 rotation_speed * delta) * Vector3.BACK;
		# do automatic rotation
#		facing_direction = lerp(facing_direction, adjusted_rotation, rotation_speed).normalized();

	# add the offset 
	var adjusted_facing := (facing_direction + (_target_rotation * facing_offset)).normalized();
#	var adjusted_facing := facing_direction;
	
	var new_rotation := Utility.get_basis_facing_direction(
				# we need to invert the directin since camera is looking toward -Z 
				-(adjusted_facing), 
				# facing should be relative to target roll
				target_up);
	var new_position := _focus_point + (_target_rotation*position_offset) - (adjusted_facing * distance);
	
#	look_at_from_position(new_position, _focus_point, target_up);
	global_transform = Transform(new_rotation, new_position);


func update_state(delta: float):
	_target = get_node(target_path) as Spatial;
	_last_manual_rotation_time += delta;
	
	_previous_focus_point = _focus_point;
	_focus_point = _target.global_transform.origin;
	
	_target_rotation = _target.global_transform.basis;
	
	facing_direction = facing_direction.normalized();


func _input(event):
	var mouse_event := event as InputEventMouseMotion;
	if !mouse_event:
		return;
	
	# negate the motion
	var mouse_motion := -mouse_event.relative;
	
	mouse_motion *= (rotation_speed * 0.2 *get_process_delta_time());
	
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
	if 1 - abs(_target_rotation.xform(new_dir+facing_offset).y) < 0.05: 
		return;
	
	facing_direction = new_dir;
	
	# disables auto align so that the manual align
	# will be respected until the timer turns it on
	_last_manual_rotation_time = 0;